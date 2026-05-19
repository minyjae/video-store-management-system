// store.Application/Services/TicketBookingService.cs
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using store.Application.Interfaces;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Application.Services;

public class TicketBookingService : ITicketBookingService
{
    private readonly ISeatRepository _seatRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer _redis;

    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

    public TicketBookingService(
        ISeatRepository seatRepository,
        ILedgerRepository ledgerRepository,
        ITicketRepository ticketRepository,
        IShowtimeRepository showtimeRepository,
        AppDbContext context,
        IConnectionMultiplexer redis)
    {
        _seatRepository    = seatRepository;
        _ledgerRepository  = ledgerRepository;
        _ticketRepository  = ticketRepository;
        _showtimeRepository = showtimeRepository;
        _context           = context;
        _redis             = redis;
    }

    public async Task<Ticket> BookSeatAsync(Guid userId, Guid seatId, Guid showtimeId)
    {
        var seat = await _seatRepository.GetByIdAsync(seatId)
            ?? throw new KeyNotFoundException($"Seat {seatId} not found.");

        var showtime = await _showtimeRepository.GetByIdAsync(showtimeId)
            ?? throw new KeyNotFoundException($"Showtime {showtimeId} not found.");

        // ── Redis Distributed Lock ───────────────────
        var lockKey   = $"seat-lock:{seatId}";
        var lockValue = userId.ToString();
        var db        = _redis.GetDatabase();

        var lockAcquired = await db.StringSetAsync(
            lockKey, lockValue, LockDuration, When.NotExists);

        if (!lockAcquired)
            throw new InvalidOperationException(
                $"Seat {seat.SeatCode} is currently being booked. Try again.");

        try
        {
            // ── เช็ค Balance ─────────────────────────
            var balance = await _ledgerRepository.GetBalanceAsync(userId);
            if (balance < seat.Price)
                throw new InvalidOperationException(
                    $"Insufficient balance. Required: {seat.Price:C}, Available: {balance:C}");

            // ── สร้าง Ticket ──────────────────────────
            var ticket = Ticket.Create(userId, showtimeId, seatId,
                seat.SeatCode, showtime.MovieName, showtime.StartTime, seat.Price);
            ticket.SetQrCode(GenerateQrCode(ticket.ReferenceCode));

            // ── EF Core Transaction ───────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. ตัดเงิน
                var ledgerEntry = LedgerEntry.CreateTicketPurchase(userId, seat.Price, ticket.Id);
                await _ledgerRepository.AppendAsync(ledgerEntry);

                // 2. เปลี่ยน Status Seat → Booked (OCC ผ่าน xmin)
                seat.Book();
                await _seatRepository.UpdateAsync(seat);

                // 3. บันทึก Ticket
                await _ticketRepository.AddAsync(ticket);

                // SaveChanges ครั้งเดียว — ทุกอย่างอยู่ใน Transaction เดียวกัน
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    $"Seat {seat.SeatCode} was just booked by someone else.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return ticket;
        }
        finally
        {
            // Release Lock เสมอ
            var current = await db.StringGetAsync(lockKey);
            if (current == lockValue)
                await db.KeyDeleteAsync(lockKey);
        }
    }

    private static string GenerateQrCode(string referenceCode)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(referenceCode);
        return Convert.ToBase64String(bytes);
    }
}