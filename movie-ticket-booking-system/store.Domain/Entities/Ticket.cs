// store.Domain/Entities/Ticket.cs
namespace store.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid ShowtimeId { get; private set; }
    public Guid SeatId { get; private set; }
    public string SeatCode { get; private set; } = string.Empty;
    public string MovieName { get; private set; } = string.Empty;
    public DateTime Showtime { get; private set; }
    public decimal PricePaid { get; private set; }
    public string ReferenceCode { get; private set; } = string.Empty;
    public string QrCodeBase64 { get; private set; } = string.Empty;
    public DateTime IssuedAt { get; private set; }

    // Navigation Properties
    public User? User { get; private set; }

    private Ticket() {}

    public static Ticket Create(Guid userId, Guid showtimeId, Guid seatId,
                                 string seatCode, string movieName,
                                 DateTime showtime, decimal pricePaid)
    {
        return new Ticket
        {
            UserId        = userId,
            ShowtimeId    = showtimeId,
            SeatId        = seatId,
            SeatCode      = seatCode,
            MovieName     = movieName,
            Showtime      = showtime,
            PricePaid     = pricePaid,
            ReferenceCode = $"TKT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            QrCodeBase64  = string.Empty,
            IssuedAt      = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok")
        };
    }

    public void SetQrCode(string qrCodeBase64)
    {
        QrCodeBase64 = qrCodeBase64;
    }
}