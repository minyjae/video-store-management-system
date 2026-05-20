// store.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;

namespace store.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Showtime> Showtimes => Set<Showtime>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<WalletSnapshot> WalletSnapshots => Set<WalletSnapshot>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Title).IsRequired().HasMaxLength(200);
            e.Property(m => m.Category).HasMaxLength(100);
            e.HasIndex(m => m.Title).IsUnique();
        });

        modelBuilder.Entity<Showtime>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Movie)
             .WithMany()
             .HasForeignKey(s => s.MovieId);
        });

        modelBuilder.Entity<Seat>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Price).HasPrecision(18, 2);
            e.Property(s => s.Type).HasConversion<string>();
            e.Property(s => s.Status).HasConversion<string>();

            // ── OCC แบบ Manual — ใช้ RowVersion Property ──
            e.Property(s => s.RowVersion)
             .IsRowVersion()
             .IsRequired();

            e.HasIndex(s => new { s.ShowtimeId, s.SeatCode }).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(100);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Role).HasConversion<string>().IsRequired();
        });

        modelBuilder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Amount).HasPrecision(18, 2);
            e.Property(l => l.Type).HasConversion<string>();
            e.HasIndex(l => l.UserId);
            e.HasOne(l => l.User)
             .WithMany(u => u.LedgerEntries)
             .HasForeignKey(l => l.UserId);
        });

        modelBuilder.Entity<WalletSnapshot>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.Balance).HasPrecision(18, 2);
            e.HasIndex(w => new { w.UserId, w.CreatedAt });
            e.HasOne(w => w.User)
             .WithMany()
             .HasForeignKey(w => w.UserId);
        });

        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.PricePaid).HasPrecision(18, 2);
            e.HasIndex(t => t.UserId);
            e.HasIndex(t => t.ReferenceCode).IsUnique();
            e.HasOne(t => t.User)
             .WithMany(u => u.Tickets)
             .HasForeignKey(t => t.UserId);
        });
    }
}