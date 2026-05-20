# 🎬 Movie Ticket Booking System — เอกสารอธิบายโค้ดฉบับสมบูรณ์

## สารบัญ

1. [ภาพรวมระบบ](#1-ภาพรวมระบบ)
2. [การเปลี่ยนจาก MongoDB → PostgreSQL](#2-การเปลี่ยนจาก-mongodb--postgresql)
3. [Clean Architecture — โครงสร้างโปรเจกต์](#3-clean-architecture--โครงสร้างโปรเจกต์)
4. [Domain Layer — หัวใจของธุรกิจ](#4-domain-layer--หัวใจของธุรกิจ)
5. [⭐ Append-Only Ledger Pattern (Wallet)](#5--append-only-ledger-pattern-wallet)
6. [⭐ Optimistic Concurrency Control (OCC)](#6--optimistic-concurrency-control-occ)
7. [⭐ Redis Distributed Lock](#7--redis-distributed-lock)
8. [⭐ PostgreSQL Transaction (EF Core)](#8--postgresql-transaction-ef-core)
9. [⭐ การทำงานร่วมกัน — Full Booking Flow](#9--การทำงานร่วมกัน--full-booking-flow)
10. [Infrastructure Layer](#10-infrastructure-layer)
11. [Application Layer](#11-application-layer)
12. [WebAPI Layer](#12-webapi-layer)
13. [Infrastructure Diagram — การ Deploy](#13-infrastructure-diagram--การ-deploy)
14. [สรุป Engineering Decisions](#14-สรุป-engineering-decisions)

---

## 1. ภาพรวมระบบ

Movie Ticket Booking System เป็นระบบจองตั๋วภาพยนตร์ที่ต้องแก้ปัญหาระดับ Production จริง 3 เรื่องหลัก:

```
ปัญหา 1: Race Condition
─────────────────────────────────────────────────────
User A และ User B กดจองที่นั่ง A1 พร้อมกัน
→ ถ้าไม่มีการป้องกัน ทั้งคู่จะได้ที่นั่งเดียวกัน
→ แก้ด้วย Redis Distributed Lock

ปัญหา 2: Concurrent Database Update
─────────────────────────────────────────────────────
2 Request อ่าน Seat พร้อมกัน แล้ว Update พร้อมกัน
→ คนหลังจะ Overwrite คนแรก (Lost Update)
→ แก้ด้วย Optimistic Concurrency Control (OCC) ผ่าน EF Core RowVersion

ปัญหา 3: Partial Failure
─────────────────────────────────────────────────────
ตัดเงินสำเร็จ แต่จองที่นั่งล้มเหลว
→ เงินหายแต่ได้ตั๋วไม่ครบ
→ แก้ด้วย PostgreSQL Transaction (All-or-Nothing, ไม่ต้องการ Replica Set)
```

---

## 2. การเปลี่ยนจาก MongoDB → PostgreSQL

### ทำไมถึงเปลี่ยน

| ปัญหาเดิม (MongoDB) | ทางออก (PostgreSQL) |
|---|---|
| Transaction ต้องการ **Replica Set** — ใช้ไม่ได้บน Standalone | PostgreSQL รองรับ **ACID Transaction** ได้ทันทีโดยไม่มีเงื่อนไข |
| Setup local dev ซับซ้อน (ต้องรัน `rs.initiate()`) | `docker compose up` แล้วใช้ได้เลย |
| Schema-less — ง่ายเกิดข้อผิดพลาดตอน Query | EF Core + Migrations บังคับ Schema ชัดเจน |
| Aggregation Pipeline ซับซ้อนสำหรับ SUM | LINQ `Sum()` อ่านง่ายกว่ามาก |

### สิ่งที่เปลี่ยนแปลง

#### Database Layer
```
เดิม: MongoDbContext.cs  → MongoDB Collections
ใหม่: AppDbContext.cs    → PostgreSQL Tables via EF Core
```

#### OCC (Optimistic Concurrency Control)
```
เดิม: Version field (int) + Manual WHERE Version=N filter ใน UpdateOne()
ใหม่: RowVersion (byte[]) + EF Core จัดการ WHERE อัตโนมัติ
      → throw DbUpdateConcurrencyException เมื่อ Conflict
```

#### Transaction
```
เดิม: IClientSessionHandle session = await client.StartSessionAsync()
      ต้องส่ง session ทุก Repository call
      ต้องการ Replica Set

ใหม่: IDbContextTransaction tx = await _context.Database.BeginTransactionAsync()
      EF Core จัดการ Connection เดียวกันอัตโนมัติ
      ไม่ต้องการ Replica Set
```

#### Ledger Balance Calculation
```
เดิม: MongoDB Aggregation Pipeline ($match + $group + $sum)
ใหม่: EF Core LINQ → context.LedgerEntries.Where(...).SumAsync(e => e.Amount)
```

#### Migrations
```
เดิม: MongoDB สร้าง Collection อัตโนมัติ ไม่ต้องทำอะไร
ใหม่: dotnet ef migrations add <Name>  → สร้างไฟล์ Migration
      db.Database.Migrate()            → รัน Auto-migration ตอน Startup
```

#### Environment Variables
```
เดิม: MONGO_USERNAME, MONGO_PASSWORD, MONGO_PORT, MONGO_DATABASE
ใหม่: POSTGRES_CONNECTION  (connection string เดียวครบ)
```

#### Timestamp Behavior (สำคัญ)
```
PostgreSQL ค่าเริ่มต้น timestamp with time zone ต้องการ Kind=Utc
แต่ TimeZoneInfo.ConvertTimeBySystemTimeZoneId() คืนค่า Kind=Unspecified

แก้ด้วย:
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
→ ใช้ timestamp without time zone แทน (รับ Kind=Unspecified ได้)
```

---

## 3. Clean Architecture — โครงสร้างโปรเจกต์

```
store/
├── store.Domain/                    ← Layer ในสุด — ไม่รู้จักใคร
│   ├── Entities/
│   │   ├── Movie.cs
│   │   ├── Showtime.cs
│   │   ├── Seat.cs                  ← มี RowVersion Field (OCC)
│   │   ├── LedgerEntry.cs           ← Append-Only Wallet
│   │   ├── WalletSnapshot.cs        ← Balance Checkpoint
│   │   ├── Ticket.cs
│   │   └── User.cs
│   ├── Interfaces/
│   │   ├── IMovieRepository.cs
│   │   ├── ISeatRepository.cs
│   │   ├── ILedgerRepository.cs
│   │   ├── ITicketRepository.cs
│   │   ├── IShowtimeRepository.cs
│   │   └── IUserRepository.cs
│   └── Enums/
│       ├── SeatStatus.cs            ← Available, Locked, Booked
│       ├── SeatType.cs              ← Normal, VIP
│       └── LedgerEntryType.cs       ← Deposit, TicketPurchase
│
├── store.Application/               ← Business Logic
│   ├── DTOs/
│   │   ├── MovieDto.cs
│   │   ├── ShowtimeDto.cs
│   │   ├── SeatDto.cs
│   │   ├── TicketDto.cs
│   │   ├── WalletDto.cs
│   │   ├── AuthDto.cs
│   │   └── BookingDto.cs
│   ├── Interfaces/
│   │   ├── IMovieService.cs
│   │   ├── IShowtimeService.cs
│   │   ├── ISeatService.cs
│   │   ├── ITicketBookingService.cs ← Orchestrator หลัก
│   │   ├── IWalletService.cs
│   │   ├── IAuthService.cs
│   │   ├── IPasswordHasher.cs
│   │   └── IJwtService.cs
│   ├── Services/
│   │   ├── TicketBookingService.cs  ← Redis Lock + PostgreSQL Transaction
│   │   ├── MovieService.cs
│   │   ├── ShowtimeService.cs
│   │   ├── SeatService.cs
│   │   ├── WalletService.cs
│   │   └── AuthService.cs
│   └── Validators/
│       ├── CreateMovieValidator.cs
│       ├── CreateShowtimeValidator.cs
│       ├── CreateSeatValidator.cs
│       ├── RegisterValidator.cs
│       ├── LoginValidator.cs
│       └── DepositValidator.cs
│
├── store.Infrastructure/            ← ติดต่อ External Systems
│   ├── Data/
│   │   └── AppDbContext.cs          ← EF Core DbContext + PostgreSQL
│   ├── Migrations/                  ← EF Core Migration Files (auto-generated)
│   ├── Repositories/
│   │   ├── SeatRepository.cs
│   │   ├── LedgerRepository.cs      ← LINQ SUM
│   │   ├── TicketRepository.cs
│   │   ├── ShowtimeRepository.cs
│   │   ├── MovieRepository.cs
│   │   └── UserRepository.cs
│   └── Services/
│       ├── PasswordHasher.cs        ← BCrypt
│       └── JwtService.cs            ← JWT Token
│
└── store.WebAPI/                    ← HTTP Layer
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── MoviesController.cs
    │   ├── ShowtimesController.cs
    │   ├── SeatsController.cs
    │   ├── BookingsController.cs    ← [Authorize] Required
    │   └── WalletController.cs      ← [Authorize] Required
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs
    └── Program.cs
```

**กฎการพึ่งพา (Dependency Rule):**
```
WebAPI → Application → Domain
Infrastructure → Domain
```
Domain ไม่รู้จักใครเลย ทำให้ Business Logic ไม่ผูกติดกับ Database หรือ Framework ใดๆ

---

## 4. Domain Layer — หัวใจของธุรกิจ

### Private Constructor + Static Factory Method Pattern

ทุก Entity ในโปรเจกต์นี้ใช้ Pattern เดียวกัน คือ ปิด Constructor และบังคับให้สร้างผ่าน `Create()`

```csharp
public class Movie
{
    // ปิด Constructor — ห้าม new Movie() จากภายนอก
    private Movie() {}

    // บังคับใช้ Factory Method ซึ่งมี Validation ครบ
    public static Movie Create(string title, ...)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Movie title is required.");

        return new Movie
        {
            Title = title.Trim(),
            CreatedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok")
        };
    }
}
```

**ทำไมถึงใช้ Pattern นี้:**
- ไม่มีทางสร้าง Object ที่ Invalid ได้เลย เพราะต้องผ่าน Validation ทุกครั้ง
- Business Rules อยู่ใน Domain ไม่กระจายไปทั่วโค้ด
- `private set` บน Properties ป้องกันการแก้ไขค่าโดยไม่ผ่าน Method

### UTC+7 (Asia/Bangkok) Timezone

ทุก Entity ใช้เวลา UTC+7 แทน UTC:

```csharp
// ใน Entity ทุกตัว
CreatedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok");
```

ต้องเปิด Legacy Timestamp Behavior ใน DependencyInjection.cs เพื่อให้ Npgsql รับ `Kind=Unspecified`:

```csharp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
```

### Enums — SeatStatus

```csharp
public enum SeatStatus
{
    Available,  // ว่าง — จองได้
    Locked,     // Redis กำลัง Lock อยู่ (ผู้ใช้กำลัง Checkout)
    Booked      // จองสำเร็จแล้ว — ถาวร
}
```

---

## 5. ⭐ Append-Only Ledger Pattern (Wallet)

### แนวคิด

ระบบ Wallet ทั่วไปเก็บ Balance เป็น Field เดียว:
```
User { Balance: 500 }
```

ปัญหาคือถ้า 2 Request อ่านค่า `500` พร้อมกัน แล้วทั้งคู่ลด 100 พร้อมกัน:
```
Request A: 500 - 100 = 400 → บันทึก 400
Request B: 500 - 100 = 400 → บันทึก 400  ← Lost Update!
ผลลัพธ์จริง: 400 (ควรเป็น 300)
```

**Append-Only Ledger แก้ปัญหานี้ด้วยการไม่มี UPDATE เลย:**

```
LedgerEntries ของ User A:
┌──────────────────────────────────────────────────┐
│ Type           │ Amount   │ Description           │
├──────────────────────────────────────────────────┤
│ Deposit        │ +1000.00 │ เติมเงิน              │
│ Deposit        │ +500.00  │ เติมเงิน              │
│ TicketPurchase │ -299.00  │ ซื้อตั๋ว Inception    │
│ TicketPurchase │ -150.00  │ ซื้อตั๋ว Avengers     │
└──────────────────────────────────────────────────┘
Balance = SUM(Amount) = 1000 + 500 - 299 - 150 = 1051.00
```

### Implementation

**Domain Entity — LedgerEntry:**
```csharp
public class LedgerEntry
{
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }  // บวก = เข้า, ลบ = ออก
    public LedgerEntryType Type { get; private set; }
    public Guid? ReferenceId { get; private set; }  // TicketId
    public DateTime CreatedAt { get; private set; }

    // Factory สำหรับเติมเงิน — Amount เป็นบวก
    public static LedgerEntry CreateDeposit(Guid userId, decimal amount) => ...;

    // Factory สำหรับซื้อตั๋ว — Amount เป็นลบ
    public static LedgerEntry CreateTicketPurchase(Guid userId, decimal amount, Guid ticketId)
        => new LedgerEntry { Amount = -amount, ... };  // ← ติดลบ!
}
```

**Infrastructure — การคำนวณ Balance ด้วย EF Core LINQ (แทน MongoDB Aggregation):**
```csharp
// เดิม (MongoDB Aggregation Pipeline — ซับซ้อน)
var pipeline = new[]
{
    new BsonDocument("$match", new BsonDocument("UserId", userId)),
    new BsonDocument("$group", new BsonDocument { ... "$sum", "$Amount" ... })
};

// ใหม่ (EF Core LINQ — อ่านง่าย)
public async Task<decimal> GetBalanceAsync(Guid userId)
    => await _context.LedgerEntries
        .Where(e => e.UserId == userId)
        .SumAsync(e => e.Amount);
```

### WalletSnapshot — ป้องกัน Full Table Scan

เมื่อ LedgerEntry มีจำนวนมาก การ `SUM()` ทุกครั้งอาจช้า ระบบจึงมี `WalletSnapshot` เป็น Checkpoint:

```csharp
public class WalletSnapshot
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }    // Balance ณ เวลานั้น
    public DateTime CreatedAt { get; private set; } // Timestamp ของ Snapshot

    // Balance จริง = Snapshot.Balance + SUM(entries หลัง Snapshot.CreatedAt)
}
```

**ทำไม Append-Only ดีกว่า:**

| | Balance Field | Append-Only Ledger |
|---|---|---|
| Race Condition | ❌ Lost Update | ✅ INSERT ไม่มี Conflict |
| Audit Trail | ❌ ไม่มีประวัติ | ✅ ทุก Transaction บันทึกครบ |
| Rollback | ❌ ต้อง UPDATE กลับ | ✅ PostgreSQL Rollback คืนทุก INSERT |
| ความซับซ้อน | ✅ ง่าย | ⚠️ Query หนักกว่า (แก้ด้วย Snapshot) |

---

## 6. ⭐ Optimistic Concurrency Control (OCC)

### ปัญหาที่แก้

```
เวลา T1: User A อ่าน Seat A1 (Status=Available, RowVersion=0xAABB)
เวลา T1: User B อ่าน Seat A1 (Status=Available, RowVersion=0xAABB)
เวลา T2: User A Update → Status=Booked ✅  (RowVersion กลายเป็น 0xCCDD)
เวลา T2: User B Update → WHERE RowVersion=0xAABB แต่ DB มี 0xCCDD แล้ว!
         → EF Core ตรวจจับ → throw DbUpdateConcurrencyException ❌
```

### RowVersion ใน Seat Entity

```csharp
public class Seat
{
    public SeatStatus Status { get; private set; }

    // EF Core จัดการ RowVersion อัตโนมัติ — ไม่ต้อง ++ เอง
    public byte[] RowVersion { get; private set; } = [];

    public void Book()
    {
        if (Status != SeatStatus.Available)
            throw new InvalidOperationException("Seat is not available.");

        Status = SeatStatus.Booked;
        // ไม่ต้อง Version++ เอง — EF Core + PostgreSQL จัดการ xmin ให้
    }
}
```

### Configuration ใน AppDbContext

```csharp
modelBuilder.Entity<Seat>(e =>
{
    e.Property(s => s.RowVersion)
     .IsRowVersion();  // ← บอก EF Core ว่านี่คือ Concurrency Token
                       // PostgreSQL ใช้ xmin column อัตโนมัติ
});
```

### EF Core จัดการ OCC อัตโนมัติ

```csharp
// เดิม (MongoDB — ต้องเขียน Filter เอง)
var filter = Builders<Seat>.Filter.And(
    Builders<Seat>.Filter.Eq(s => s.Id, seat.Id),
    Builders<Seat>.Filter.Eq(s => s.Version, expectedVersion) // Manual!
);
var result = await _collection.UpdateOneAsync(session, filter, update);
if (result.ModifiedCount == 0) throw new InvalidOperationException("Concurrent booking!");

// ใหม่ (EF Core — อัตโนมัติ)
_context.Seats.Update(seat);
await _context.SaveChangesAsync();
// EF Core สร้าง SQL: UPDATE seats SET ... WHERE id=? AND xmin=?
// ถ้าไม่มีแถวเปลี่ยน → throw DbUpdateConcurrencyException อัตโนมัติ
```

### การ Handle ใน Service

```csharp
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    // OCC ตรวจจับ Conflict — มีคนจองก่อนแล้ว
    throw new InvalidOperationException("Seat was booked by another user. Please try again.");
}
```

**OCC vs Pessimistic Lock (SELECT FOR UPDATE):**

| | OCC | Pessimistic Lock |
|---|---|---|
| Throughput | ✅ สูง (ไม่ Lock Row) | ❌ ต่ำ (Lock ตลอด) |
| Deadlock | ✅ ไม่มี | ❌ อาจเกิด |
| เหมาะกับ | Conflict น้อย | Conflict บ่อย |
| EF Core Support | ✅ IsRowVersion() | ✅ FromSqlRaw("SELECT ... FOR UPDATE") |

---

## 7. ⭐ Redis Distributed Lock

### ทำไมต้อง Distributed Lock

OCC แก้ปัญหาระดับ Database แต่ถ้าไม่มี Lock ผู้ใช้หลายคนจะผ่าน Application Layer มาพร้อมกัน แล้วแข่งกัน Update PostgreSQL ซึ่งแม้ OCC จะจับได้ แต่ประสบการณ์ผู้ใช้แย่มาก (Error บ่อย)

Redis Lock แก้ที่ต้นทาง — ให้แค่คนเดียวเข้าถึง Booking Flow ของที่นั่งนั้นได้

### SET NX — Atomic Lock Operation

```
คำสั่ง: SET seat-lock:seatId userId NX PX 300000

NX  = SET if Not eXists (ถ้าไม่มี Key นี้ค่อย SET)
PX  = Expire ใน milliseconds (300000ms = 5 นาที)
```

```csharp
var lockAcquired = await db.StringSetAsync(
    key:    $"seat-lock:{seatId}",
    value:  userId,             // เก็บ userId เพื่อรู้ว่าใครถือ Lock
    expiry: TimeSpan.FromMinutes(5),
    when:   When.NotExists      // ← NX — Atomic!
);

if (!lockAcquired)
    throw new InvalidOperationException("Seat is being booked by another user.");
```

**Timeline ของ Lock:**
```
T=0:00  User A ได้ Lock → Redis: seat-lock:A1 = "userA" (TTL 5 นาที)
T=0:00  User B พยายาม Lock → NX Fail → Error "กำลังถูกจองอยู่"

T=0:30  User A ชำระเงินสำเร็จ → Delete Lock → Redis: seat-lock:A1 ถูกลบ
T=0:30  User B (คนอื่น) ลองใหม่ → ได้ Lock สำเร็จ

--- กรณี Server ตาย ---
T=0:00  User A ได้ Lock
T=1:00  Server A ตาย → Lock ไม่ได้ถูก Release
T=5:00  TTL หมดอายุ → Redis ลบ Lock อัตโนมัติ ✅
T=5:00  User B ลองใหม่ → ได้ Lock สำเร็จ
```

### Release Lock อย่างปลอดภัย

```csharp
finally
{
    // เช็คก่อนว่า Lock ยังเป็นของเราไหม
    // (ป้องกัน Edge Case ที่ Lock หมดอายุแล้ว คนอื่น Lock แทน)
    var currentValue = await db.StringGetAsync(lockKey);
    if (currentValue == lockValue)
    {
        await db.KeyDeleteAsync(lockKey);
    }
    // ถ้าไม่ใช่ของเรา = Lock หมดอายุไปแล้ว ไม่ต้อง Delete
}
```

---

## 8. ⭐ PostgreSQL Transaction (EF Core)

### ทำไมถึงดีกว่า MongoDB Transaction

```
MongoDB Transaction (เดิม):
├── ต้องการ Replica Set (แม้ใน Local Dev)
├── Setup ซับซ้อน — ต้องรัน rs.initiate() เอง
├── ต้องส่ง IClientSessionHandle เข้าทุก Repository
└── ถ้าลืมส่ง session → Operation ไม่อยู่ใน Transaction!

PostgreSQL Transaction (ใหม่):
├── ACID ได้ทันทีโดยไม่มีเงื่อนไข — Standalone ก็ใช้ได้
├── docker compose up → ใช้ได้เลย
├── EF Core ใช้ Connection เดียวกันอัตโนมัติ ไม่ต้องส่ง session
└── SaveChangesAsync() รวบทุก Operation ใน Transaction เดียว
```

### Booking Flow มี 3 Operations ที่ต้องสำเร็จพร้อมกันทั้งหมด

```
1. INSERT LedgerEntry (ตัดเงิน)
2. UPDATE Seat Status → Booked  (พร้อม OCC Check)
3. INSERT Ticket

ถ้า Operation 2 ล้มเหลว:
→ เงินถูกตัดไปแล้ว (Operation 1 สำเร็จ)
→ แต่ Seat ยังว่างอยู่
→ ไม่มี Ticket
→ เงินหาย!
```

### Implementation

```csharp
// เดิม (MongoDB) — ต้องส่ง session ทุกที่
using var session = await _mongoContext.Client.StartSessionAsync();
session.StartTransaction();
try
{
    await _ledgerRepository.AppendAsync(ledgerEntry, session);   // ← session
    await _seatRepository.BookSeatWithOccAsync(seat, session);   // ← session
    await _ticketRepository.AddAsync(ticket, session);           // ← session
    await session.CommitTransactionAsync();
}
catch { await session.AbortTransactionAsync(); throw; }

// ใหม่ (EF Core + PostgreSQL) — สะอาดกว่ามาก
await using var tx = await _context.Database.BeginTransactionAsync();
try
{
    _context.LedgerEntries.Add(ledgerEntry);
    _context.Seats.Update(seat);
    _context.Tickets.Add(ticket);

    await _context.SaveChangesAsync();  // ← Atomic: ทุกอย่างหรือไม่มีเลย
    await tx.CommitAsync();             // ✅ บันทึก
}
catch
{
    await tx.RollbackAsync();           // ❌ ยกเลิกทั้งหมด
    throw;
}
```

### ACID Properties ที่ PostgreSQL รับประกัน

| Property | ความหมาย | ผลใน Booking |
|---|---|---|
| **Atomicity** | ทุกอย่างหรือไม่มีเลย | ถ้า INSERT Ticket ล้มเหลว LedgerEntry ก็ Rollback |
| **Consistency** | ข้อมูลถูกต้องเสมอ | Balance ไม่ติดลบ, Seat ไม่ Booked 2 คน |
| **Isolation** | Transaction ไม่เห็นกัน | User B ไม่เห็น LedgerEntry ของ User A ที่ยังไม่ Commit |
| **Durability** | Commit แล้วหายไม่ได้ | แม้ Server ตาย ข้อมูลยังอยู่ |

### Auto-Migration on Startup

```csharp
// Program.cs — รัน Migration อัตโนมัติทุกครั้งที่ Start
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();  // Apply pending migrations
}
```

ทำให้ไม่ต้องรัน `dotnet ef database update` ด้วยตนเองทุกครั้ง

---

## 9. ⭐ การทำงานร่วมกัน — Full Booking Flow

```
POST /api/bookings
Body: { "seatId": "...", "showtimeId": "..." }

│
▼
BookingsController.Book()
├── ดึง UserId จาก JWT Token (ไม่รับจาก Body — ปลอดภัยกว่า)
└── เรียก TicketBookingService.BookSeatAsync()
    │
    ├── Step 1: ดึงข้อมูล Seat และ Showtime จาก PostgreSQL (EF Core)
    │
    ├── Step 2: Redis Distributed Lock
    │   ├── SET seat-lock:{seatId} NX TTL=5min
    │   ├── ✅ ได้ Lock → ไปต่อ
    │   └── ❌ ไม่ได้ Lock → throw "ที่นั่งกำลังถูกจองอยู่" → 400
    │
    ├── Step 3: เช็ค Wallet Balance
    │   ├── SUM(amount) จาก LedgerEntries (LINQ)
    │   ├── ✅ พอจ่าย → ไปต่อ
    │   └── ❌ ไม่พอ → throw "Insufficient balance" → 400
    │
    ├── Step 4: PostgreSQL Transaction (BeginTransactionAsync)
    │   ├── INSERT LedgerEntry (amount = -price)
    │   ├── UPDATE Seat (Status=Booked)      ← OCC Check (RowVersion)
    │   ├── INSERT Ticket
    │   │
    │   ├── SaveChangesAsync()
    │   │   ├── ✅ สำเร็จ → CommitAsync
    │   │   └── ❌ DbUpdateConcurrencyException → RollbackAsync → throw
    │   │
    │   └── ❌ อะไรก็ตาม → RollbackAsync → ยกเลิกทั้งหมด
    │
    └── Step 5: Release Redis Lock (finally block — ทำเสมอ)
        │
        └── คืน TicketDto กลับไปให้ Client → 200 OK
```

### 3 Layer Defense (เหมือนเดิม แต่ใช้ PostgreSQL แทน MongoDB)

```
Layer 1: Redis Distributed Lock
└── ป้องกันไม่ให้หลาย Request เข้าสู่ Booking Flow พร้อมกัน

Layer 2: OCC (EF Core RowVersion)
└── ป้องกัน Lost Update ระดับ Database (Defense in Depth)
    throw DbUpdateConcurrencyException อัตโนมัติ

Layer 3: PostgreSQL Transaction
└── ป้องกัน Partial Failure — Rollback ทุกอย่างถ้ามีอะไรผิดพลาด
    ไม่ต้องการ Replica Set ทำงานได้บน Standalone
```

---

## 10. Infrastructure Layer

### AppDbContext — EF Core Configuration

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Showtime> Showtimes => Set<Showtime>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<WalletSnapshot> WalletSnapshots => Set<WalletSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // OCC — PostgreSQL ใช้ xmin system column
        modelBuilder.Entity<Seat>()
            .Property(s => s.RowVersion)
            .IsRowVersion();

        // Unique Index — ป้องกัน Movie Title ซ้ำระดับ DB
        modelBuilder.Entity<Movie>()
            .HasIndex(m => m.Title)
            .IsUnique();
    }
}
```

### Unique Constraint — Movie Title

ระบบใช้ 2 Layer เพื่อป้องกัน Title ซ้ำ:

```
Layer 1: Application Check (MovieService.CreateAsync)
├── CheckMovieExistAsync() — ค้นหาด้วย FirstOrDefaultAsync
├── ถ้าเจอ → throw ArgumentException("Movie title is already in use.")
└── เร็วและ Error Message ชัดเจน

Layer 2: DB Unique Index (AppDbContext)
├── HasIndex(m => m.Title).IsUnique()
├── ถ้า Race Condition ทำให้ Layer 1 ผ่าน → DB จะ Reject
└── catch DbUpdateException → throw ArgumentException (Safety Net)
```

### Password Hashing — BCrypt

```csharp
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
```

### JWT Service

```csharp
public string GenerateToken(Guid userId, string email)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!));

    var token = new JwtSecurityToken(
        claims: [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),  // ← Guid
            new Claim(ClaimTypes.Email, email)
        ],
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

## 11. Application Layer

### DTO Pattern — ทำไมไม่ส่ง Entity ตรงๆ

```csharp
// Entity — มีทุกอย่าง รวมถึง RowVersion (ไม่ควรส่งให้ Client)
public class Seat
{
    public Guid Id { get; }
    public string SeatCode { get; }
    public SeatStatus Status { get; }
    public byte[] RowVersion { get; }   // ← ไม่ควรเปิดเผย (Internal OCC)
}

// DTO — ส่งเฉพาะที่ต้องการ
public record SeatDto(
    Guid Id,
    string SeatCode,
    SeatType Type,
    decimal Price,
    SeatStatus Status    // ← Client ใช้แสดงสี Available/Locked/Booked
);
```

### FluentValidation

```csharp
public class DepositValidator : AbstractValidator<DepositDto>
{
    public DepositValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Deposit amount must be positive.")
            .LessThanOrEqualTo(100000).WithMessage("Max deposit is 100,000.");
    }
}
```

Validation ทำ 2 ชั้น:
1. **FluentValidation** (Application Layer) — ตรวจ Input Format
2. **Domain Validation** (Domain Layer) — ตรวจ Business Rules ใน `Create()`

---

## 12. WebAPI Layer

### [Authorize] — ป้องกัน Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]                  // ← ทุก Endpoint ใน Controller นี้ต้องมี JWT Token
public class BookingsController : ControllerBase
```

### ดึง UserId จาก JWT Token (เป็น Guid)

```csharp
// ไม่รับ UserId จาก Request Body
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? throw new UnauthorizedAccessException("User not found in token.");

// Parse string → Guid (PostgreSQL ใช้ Guid ไม่ใช่ string)
var userGuid = Guid.Parse(userId);
await _bookingService.BookSeatAsync(userGuid, dto);
```

### CORS — อนุญาต Frontend

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:4200")  // Angular Dev Server
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors("Frontend");  // ต้องอยู่ก่อน UseAuthentication
```

### ExceptionHandlingMiddleware — Error Mapping

```csharp
var (statusCode, message) = exception switch
{
    ValidationException         => (400, exception.Message),
    ArgumentException           => (400, exception.Message),   // Domain Validation
    KeyNotFoundException        => (404, exception.Message),
    InvalidOperationException   => (400, exception.Message),   // Business Rules
    UnauthorizedAccessException => (401, exception.Message),
    _                           => (500, "เกิดข้อผิดพลาด กรุณาลองใหม่")
};
```

---

## 13. Infrastructure Diagram — การ Deploy

### Development (Local)

```
┌─────────────────────────────────────────────────────┐
│                   Developer Machine                  │
│                                                      │
│  ┌──────────────────┐    ┌──────────────────────┐   │
│  │   .NET WebAPI    │───▶│  Docker Compose       │   │
│  │   (Port 5000)    │    │                       │   │
│  └──────────────────┘    │  ┌─────────────────┐ │   │
│                           │  │  PostgreSQL 16   │ │   │
│  .env                     │  │  (Port 5432)     │ │   │
│  POSTGRES_CONNECTION=...  │  └─────────────────┘ │   │
│  REDIS_CONNECTION=...     │                       │   │
│  JWT_SECRET=...           │  ┌─────────────────┐ │   │
│                           │  │     Redis 7      │ │   │
│                           │  │   (Port 6379)    │ │   │
│                           │  └─────────────────┘ │   │
│                           └──────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

**docker-compose.yml สำหรับ Development:**
```yaml
services:
  postgres:
    image: postgres:16
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: password
      POSTGRES_DB: movieticket
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  postgres_data:
```

> **ข้อได้เปรียบเหนือ MongoDB**: PostgreSQL ไม่ต้องทำ `rs.initiate()` หรือ configure Replica Set เพื่อใช้ Transaction — `docker compose up` แล้วใช้ได้ทันที

---

### Production (Cloud — Recommended Architecture)

```
                         Internet
                            │
                            ▼
                   ┌─────────────────┐
                   │   Cloudflare    │
                   │  (CDN / DDoS)   │
                   └────────┬────────┘
                            │ HTTPS
                            ▼
                   ┌─────────────────┐
                   │  Load Balancer  │
                   │  (Nginx / ALB)  │
                   └────────┬────────┘
                            │
              ┌─────────────┼─────────────┐
              │             │             │
              ▼             ▼             ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │  .NET WebAPI │ │  .NET WebAPI │ │  .NET WebAPI │
    │  Instance 1  │ │  Instance 2  │ │  Instance 3  │
    │  (Container) │ │  (Container) │ │  (Container) │
    └──────┬───────┘ └──────┬───────┘ └──────┬───────┘
           │                │                │
           └────────────────┼────────────────┘
                            │
              ┌─────────────┴─────────────┐
              │                           │
              ▼                           ▼
    ┌──────────────────┐       ┌──────────────────────┐
    │  Redis Cluster   │       │  PostgreSQL           │
    │  (Distributed    │       │  Primary + Standby    │
    │   Locking Only)  │       │  (Streaming           │
    │                  │       │   Replication)        │
    │  Master          │       │                       │
    │  Replica x2      │       │  Primary    Standby   │
    └──────────────────┘       │     │          │      │
                               │     ▼          ▼      │
                               │  [Write]   [Failover] │
                               └──────────────────────┘
```

### ทำไมถึงไม่ต้องการ PostgreSQL Replica Set สำหรับ Transaction

```
MongoDB:
└── Transaction ต้องการ Replica Set บังคับ
    → ถ้าใช้ Standalone → Transaction ใช้ไม่ได้เลย

PostgreSQL:
└── Transaction เป็น Core Feature ของ RDBMS
    → Standalone ก็ใช้ได้เต็มประสิทธิภาพ
    → Replication เพิ่มเฉพาะเพื่อ High Availability / Read Scaling
```

### Horizontal Scaling ทำงานอย่างไร

ด้วย Redis Distributed Lock ระบบ Scale ได้โดยไม่มีปัญหา Race Condition:
```
User A → Pod 1 → ขอ Redis Lock seat-lock:A1 → ✅ ได้
User B → Pod 2 → ขอ Redis Lock seat-lock:A1 → ❌ ไม่ได้ (Pod ไหนก็ไม่ได้)
User C → Pod 3 → ขอ Redis Lock seat-lock:A1 → ❌ ไม่ได้

Lock อยู่ที่ Redis (Shared) ไม่ใช่ Memory ของแต่ละ Pod
```

---

## 14. สรุป Engineering Decisions

### Design Patterns ที่ใช้

| Pattern | ใช้ที่ไหน | แก้ปัญหาอะไร |
|---|---|---|
| **Append-Only Ledger** | Wallet / LedgerEntry | Lost Update บน Balance, Audit Trail |
| **Optimistic Concurrency Control** | Seat Booking (EF Core RowVersion) | Concurrent Update บน Row เดียวกัน |
| **Distributed Lock** | Redis | Race Condition ระหว่าง Application Instances |
| **PostgreSQL Transaction** | Booking Flow | Partial Failure — All-or-Nothing, ไม่ต้องการ Replica Set |
| **Static Factory Method** | ทุก Entity | Validation ก่อนสร้าง Object |
| **Repository Pattern** | Infrastructure | แยก DB Logic ออกจาก Business Logic |
| **Middleware Pipeline** | Exception Handling | จัดการ Error กลางที่เดียว |
| **DTO Pattern** | Application Layer | ซ่อน Internal Fields (RowVersion, etc.) |
| **WalletSnapshot** | Ledger | ป้องกัน Full Table Scan เมื่อ Entry มีจำนวนมาก |

### ลำดับการป้องกัน Race Condition

```
Layer 1: Redis Distributed Lock
└── ป้องกันไม่ให้หลาย Request เข้าสู่ Booking Flow พร้อมกัน

Layer 2: EF Core OCC (RowVersion + DbUpdateConcurrencyException)
└── ป้องกัน Lost Update ระดับ Database (Defense in Depth)

Layer 3: PostgreSQL Transaction (BeginTransactionAsync)
└── ป้องกัน Partial Failure — Rollback ทุกอย่างถ้ามีอะไรผิดพลาด

3 Layer ทำงานร่วมกัน → ระบบปลอดภัยแม้มี Load สูง
```

### Environment Variables

```env
# PostgreSQL
POSTGRES_CONNECTION=Host=localhost;Port=5432;Database=movieticket;Username=admin;Password=password

# Redis (Distributed Lock only — ไม่ใช้สำหรับ Caching)
REDIS_CONNECTION=localhost:6379

# JWT
JWT_SECRET=your-super-secret-key-minimum-32-characters
```

### Packages ที่ใช้

| Package | ใช้ทำอะไร |
|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core Provider สำหรับ PostgreSQL |
| `Microsoft.EntityFrameworkCore.Tools` | EF Core Migrations CLI |
| `StackExchange.Redis` | Redis Distributed Lock |
| `BCrypt.Net-Next` | Password Hashing |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT Authentication |
| `FluentValidation.AspNetCore` | Input Validation |
| `DotNetEnv` | โหลด .env file |

### EF Core Migration Commands

```bash
# รันจาก movie-ticket-booking-system/

# สร้าง migration ใหม่
dotnet ef migrations add <MigrationName> --project store.Infrastructure --startup-project store.WebAPI

# Apply migration (ปกติไม่ต้องรัน เพราะ auto-migrate ใน Program.cs)
dotnet ef database update --project store.Infrastructure --startup-project store.WebAPI

# ย้อนกลับ migration ล่าสุด
dotnet ef migrations remove --project store.Infrastructure --startup-project store.WebAPI

# ดู migration ทั้งหมด
dotnet ef migrations list --project store.Infrastructure --startup-project store.WebAPI

# Drop และสร้าง Database ใหม่
dotnet ef database drop --project store.Infrastructure --startup-project store.WebAPI
dotnet ef database update --project store.Infrastructure --startup-project store.WebAPI
```
