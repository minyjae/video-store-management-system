// store.Domain/Entities/Seat.cs
using store.Domain.Enums;
public class Seat
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ShowtimeId { get; private set; }
    public string SeatCode { get; private set; } = string.Empty;
    public SeatType Type { get; private set; }
    public decimal Price { get; private set; }
    public SeatStatus Status { get; private set; }

    // OCC — EF Core อัปเดตค่านี้อัตโนมัติทุกครั้งที่ Save
    public byte[] RowVersion { get; private set; } = [];

    private Seat() {}

    public static Seat Create(Guid showtimeId, string seatCode,
                               SeatType type, decimal price)
    {
        return new Seat
        {
            ShowtimeId = showtimeId,
            SeatCode   = seatCode,
            Type       = type,
            Price      = price,
            Status     = SeatStatus.Available
        };
    }

    public void Book()
    {
        if (Status != SeatStatus.Available)
            throw new InvalidOperationException(
                $"Seat {SeatCode} is not available. Current: {Status}");
        Status = SeatStatus.Booked;
    }
}