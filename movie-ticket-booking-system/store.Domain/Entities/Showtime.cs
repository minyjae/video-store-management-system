// store.Domain/Entities/Showtime.cs
namespace store.Domain.Entities;

public class Showtime
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid MovieId { get; private set; }
    public string MovieName { get; private set; } = string.Empty;
    public string ScreenId { get; private set; } = string.Empty;
    public string ScreenName { get; private set; } = string.Empty;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    // Navigation Property สำหรับ EF Core
    public Movie? Movie { get; private set; }

    private Showtime() {}

    public static Showtime Create(Guid movieId, string movieName,
                                   string screenId, string screenName,
                                   DateTime startTime, int durationMinutes)
    {
        if (startTime < TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok"))
            throw new ArgumentException("Showtime cannot be in the past.");

        return new Showtime
        {
            MovieId    = movieId,
            MovieName  = movieName,
            ScreenId   = screenId,
            ScreenName = screenName,
            StartTime  = startTime,
            EndTime    = startTime.AddMinutes(durationMinutes)
        };
    }
}