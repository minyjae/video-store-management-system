# 🎬 Movie Ticket Booking System — เอกสารอธิบายโค้ดฉบับสมบูรณ์

## สารบัญ

1. [ภาพรวมระบบ](#1-ภาพรวมระบบ)
2. [Clean Architecture — โครงสร้างโปรเจกต์](#2-clean-architecture--โครงสร้างโปรเจกต์)
3. [Domain Layer — หัวใจของธุรกิจ](#3-domain-layer--หัวใจของธุรกิจ)
4. [⭐ Append-Only Ledger Pattern (Wallet)](#4--append-only-ledger-pattern-wallet)
5. [⭐ Optimistic Concurrency Control (OCC)](#5--optimistic-concurrency-control-occ)
6. [⭐ Redis Distributed Lock](#6--redis-distributed-lock)
7. [⭐ MongoDB Transaction](#7--mongodb-transaction)
8. [⭐ การทำงานร่วมกัน — Full Booking Flow](#8--การทำงานร่วมกัน--full-booking-flow)
9. [Infrastructure Layer](#9-infrastructure-layer)
10. [Application Layer](#10-application-layer)
11. [WebAPI Layer](#11-webapi-layer)
12. [Infrastructure Diagram — การ Deploy](#12-infrastructure-diagram--การ-deploy)
13. [สรุป Engineering Decisions](#13-สรุป-engineering-decisions)

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
→ แก้ด้วย Optimistic Concurrency Control (OCC)

ปัญหา 3: Partial Failure
─────────────────────────────────────────────────────
ตัดเงินสำเร็จ แต่จองที่นั่งล้มเหลว
→ เงินหายแต่ได้ตั๋วไม่ครบ
→ แก้ด้วย MongoDB Transaction (All-or-Nothing)
```

---

## 2. Clean Architecture — โครงสร้างโปรเจกต์

```
store/
├── store.Domain/                    ← Layer ในสุด — ไม่รู้จักใคร
│   ├── Entities/
│   │   ├── Movie.cs
│   │   ├── Showtime.cs
│   │   ├── Seat.cs                  ← มี Version Field (OCC)
│   │   ├── LedgerEntry.cs           ← Append-Only Wallet
│   │   ├── Ticket.cs
│   │   └── User.cs
│   ├── Interfaces/
│   │   ├── IMovieRepository.cs
│   │   ├── ISeatRepository.cs       ← มี OCC Method
│   │   ├── ILedgerRepository.cs     ← มี Session Overload
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
│   │   ├── TicketBookingService.cs  ← Redis Lock + MongoDB Transaction
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
│   │   └── MongoDbContext.cs        ← MongoDB + Indexes
│   ├── Repositories/
│   │   ├── SeatRepository.cs        ← OCC Implementation
│   │   ├── LedgerRepository.cs      ← Aggregation Pipeline
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

## 3. Domain Layer — หัวใจของธุรกิจ

### Private Constructor + Static Factory Method Pattern

ทุก Entity ในโปรเจกต์นี้ใช้ Pattern เดียวกัน คือ ปิด Constructor และบังคับให้สร้างผ่าน `Create()`

```csharp
public class Movie
{
    // ปิด Constructor — ห้าม new Movie() จากภายนอก
    private Movie() {}

    // บังคับใช้ Factory Method ซึ่งมี Validation ครบ
    public static Movie Create(string name, ...)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Movie name is required.");

        return new Movie { Name = name.Trim(), ... };
    }
}
```

**ทำไมถึงใช้ Pattern นี้:**
- ไม่มีทางสร้าง Object ที่ Invalid ได้เลย เพราะต้องผ่าน Validation ทุกครั้ง
- Business Rules อยู่ใน Domain ไม่กระจายไปทั่วโค้ด
- `private set` บน Properties ป้องกันการแก้ไขค่าโดยไม่ผ่าน Method

### Enums — SeatStatus

```csharp
public enum SeatStatus
{
    Available,  // ว่าง — จองได้
    Locked,     // Redis กำลัง Lock อยู่ (ผู้ใช้กำลัง Checkout)
    Booked      // จองสำเร็จแล้ว — ถาวร
}
```

`Locked` ไม่ได้ใช้ใน MongoDB โดยตรง แต่ใช้แสดงสถานะบน UI ให้ผู้ใช้เห็นว่าที่นั่งนี้กำลังถูกจองอยู่

---

## 4. ⭐ Append-Only Ledger Pattern (Wallet)

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
    public string UserId { get; private set; }
    public decimal Amount { get; private set; }  // บวก = เข้า, ลบ = ออก
    public LedgerEntryType Type { get; private set; }
    public string? ReferenceId { get; private set; }  // TicketId
    public DateTime CreatedAt { get; private set; }

    // Factory สำหรับเติมเงิน — Amount เป็นบวก
    public static LedgerEntry CreateDeposit(string userId, decimal amount)
        => new LedgerEntry { Amount = amount, ... };

    // Factory สำหรับซื้อตั๋ว — Amount เป็นลบ
    public static LedgerEntry CreateTicketPurchase(string userId, decimal amount, string ticketId)
        => new LedgerEntry { Amount = -amount, ... };  // ← ติดลบ!
}
```

**Infrastructure — การคำนวณ Balance ด้วย MongoDB Aggregation:**
```csharp
public async Task<decimal> GetBalanceAsync(string userId)
{
    // แทนที่จะ SELECT balance FROM users
    // เราทำ SELECT SUM(amount) FROM ledger_entries WHERE user_id = ?
    var pipeline = new[]
    {
        // Stage 1: กรองเฉพาะ User นี้
        new BsonDocument("$match", new BsonDocument("UserId", userId)),

        // Stage 2: รวม Amount ทั้งหมด
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", BsonNull.Value },
            { "total", new BsonDocument("$sum", "$Amount") }
        })
    };

    var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
    return result?["total"].AsDecimal ?? 0m;
}
```

**ทำไม Append-Only ดีกว่า:**

| | Balance Field | Append-Only Ledger |
|---|---|---|
| Race Condition | ❌ Lost Update | ✅ INSERT ไม่มี Conflict |
| Audit Trail | ❌ ไม่มีประวัติ | ✅ ทุก Transaction บันทึกครบ |
| Rollback | ❌ ต้อง UPDATE กลับ | ✅ ลบ Entry ล่าสุดออก |
| ความซับซ้อน | ✅ ง่าย | ⚠️ Query หนักกว่า |

---

## 5. ⭐ Optimistic Concurrency Control (OCC)

### ปัญหาที่แก้

```
เวลา T1: User A อ่าน Seat A1 (Status=Available, Version=0)
เวลา T1: User B อ่าน Seat A1 (Status=Available, Version=0)
เวลา T2: User A Update → Status=Booked, Version=1 ✅
เวลา T2: User B Update → Status=Booked, Version=1
         แต่ Version ใน DB เป็น 1 แล้ว ไม่ใช่ 0!
         → OCC ตรวจจับได้ → Reject ❌
```

### Version Field ใน Seat Entity

```csharp
public class Seat
{
    public SeatStatus Status { get; private set; }

    // Version เพิ่มขึ้นทุกครั้งที่มีการเปลี่ยนแปลง
    public int Version { get; private set; } = 0;

    public void Book()
    {
        if (Status != SeatStatus.Available)
            throw new InvalidOperationException("Seat is not available.");

        Status = SeatStatus.Booked;
        Version++;  // ← bump version ทุกครั้งที่ Book
    }
}
```

### OCC Update ใน Repository

```csharp
public async Task BookSeatWithOccAsync(Seat seat, IClientSessionHandle session)
{
    // Version ที่เราดึงมาจาก DB ก่อน Book()
    var expectedVersion = seat.Version - 1;

    var filter = Builders<Seat>.Filter.And(
        Builders<Seat>.Filter.Eq(s => s.Id, seat.Id),
        Builders<Seat>.Filter.Eq(s => s.Version, expectedVersion) // ← เช็ค Version
    );

    var update = Builders<Seat>.Update
        .Set(s => s.Status, seat.Status)
        .Set(s => s.Version, seat.Version);

    var result = await _collection.UpdateOneAsync(session, filter, update);

    if (result.ModifiedCount == 0)
    {
        // Version ไม่ตรง = มีคนอื่น Update ไปก่อนแล้ว
        throw new InvalidOperationException("Concurrent booking detected!");
    }
}
```

**การทำงาน Step by Step:**
```
1. User A และ B ดึง Seat มา: Version=0
2. User A เรียก Book() → Version กลายเป็น 1
3. User A Update: WHERE Id=X AND Version=0 → สำเร็จ ModifiedCount=1
   DB: Version ตอนนี้เป็น 1

4. User B เรียก Book() → Version กลายเป็น 1
5. User B Update: WHERE Id=X AND Version=0 → ไม่เจอ Document!
   เพราะ DB เป็น Version=1 แล้ว → ModifiedCount=0
   → throw Exception → Rollback Transaction
```

**OCC vs Pessimistic Lock (SELECT FOR UPDATE):**

| | OCC | Pessimistic Lock |
|---|---|---|
| Throughput | ✅ สูง (ไม่ Lock Row) | ❌ ต่ำ (Lock ตลอด) |
| Deadlock | ✅ ไม่มี | ❌ อาจเกิด |
| เหมาะกับ | Conflict น้อย | Conflict บ่อย |
| Complexity | ⚠️ ต้อง Handle Retry | ✅ ง่ายกว่า |

---

## 6. ⭐ Redis Distributed Lock

### ทำไมต้อง Distributed Lock

OCC แก้ปัญหาระดับ Database แต่ถ้าไม่มี Lock ผู้ใช้หลายคนจะผ่าน Application Layer มาพร้อมกัน แล้วแข่งกัน Update MongoDB ซึ่งแม้ OCC จะจับได้ แต่ประสบการณ์ผู้ใช้แย่มาก (Error บ่อย)

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

## 7. ⭐ MongoDB Transaction

### ทำไมต้องใช้ Transaction

Booking Flow มี 3 Operations ที่ต้องสำเร็จพร้อมกันทั้งหมด:

```
1. INSERT LedgerEntry (ตัดเงิน)
2. UPDATE Seat Status → Booked
3. INSERT Ticket

ถ้า Operation 2 ล้มเหลว:
→ เงินถูกตัดไปแล้ว (Operation 1 สำเร็จ)
→ แต่ Seat ยังว่างอยู่
→ ไม่มี Ticket
→ เงินหาย!
```

Transaction แก้ด้วยหลักการ All-or-Nothing

### Implementation

```csharp
using var session = await _mongoContext.Client.StartSessionAsync();
session.StartTransaction();

try
{
    // ทุก Operation ส่ง session เข้าไปด้วย
    await _ledgerRepository.AppendAsync(ledgerEntry, session);
    await _seatRepository.BookSeatWithOccAsync(seat, session);
    await _ticketRepository.AddAsync(ticket, session);

    await session.CommitTransactionAsync();  // ✅ ทุกอย่าง OK → บันทึก
}
catch
{
    await session.AbortTransactionAsync();   // ❌ มีอะไรผิดพลาด → ยกเลิกทั้งหมด
    throw;
}
```

**ข้อกำหนดของ MongoDB Transaction:**
- ต้องใช้ **MongoDB 4.0+**
- ต้องเป็น **Replica Set** (ไม่ใช่ Standalone) แม้แต่ใน Development
- `MongoDbContext` ต้อง expose `IMongoClient` เพื่อ `StartSessionAsync()`

---

## 8. ⭐ การทำงานร่วมกัน — Full Booking Flow

```
POST /api/bookings
Body: { "seatId": "abc", "showtimeId": "xyz" }

│
▼
BookingsController.Book()
├── ดึง UserId จาก JWT Token (ไม่รับจาก Body — ปลอดภัยกว่า)
└── เรียก TicketBookingService.BookSeatAsync()
    │
    ├── Step 1: ดึงข้อมูล Seat และ Showtime จาก MongoDB
    │
    ├── Step 2: Redis Distributed Lock
    │   ├── SET seat-lock:{seatId} NX TTL=5min
    │   ├── ✅ ได้ Lock → ไปต่อ
    │   └── ❌ ไม่ได้ Lock → throw "ที่นั่งกำลังถูกจองอยู่" → 500
    │
    ├── Step 3: เช็ค Wallet Balance
    │   ├── SUM(amount) จาก LedgerEntries
    │   ├── ✅ พอจ่าย → ไปต่อ
    │   └── ❌ ไม่พอ → throw "Insufficient balance" → 500
    │
    ├── Step 4: สร้าง Ticket Object + Generate QR Code
    │
    ├── Step 5: MongoDB Transaction
    │   ├── INSERT LedgerEntry (amount = -price)
    │   ├── UPDATE Seat (Status=Booked, Version++) ← OCC Check
    │   ├── INSERT Ticket
    │   │
    │   ├── ✅ ทุกอย่าง OK → CommitTransaction
    │   └── ❌ มีอะไรผิด → AbortTransaction → Rollback ทุกอย่าง
    │
    └── Step 6: Release Redis Lock (finally block — ทำเสมอ)
        │
        └── คืน TicketDto กลับไปให้ Client → 200 OK
```

---

## 9. Infrastructure Layer

### MongoDbContext — การสร้าง Indexes

```csharp
private void EnsureIndexes()
{
    // Seat: Unique Index บน ShowtimeId + SeatCode
    // ป้องกันที่นั่งซ้ำใน Showtime เดียวกันระดับ DB
    var seatIndex = Builders<Seat>.IndexKeys
        .Ascending(s => s.ShowtimeId)
        .Ascending(s => s.SeatCode);
    seatCollection.Indexes.CreateOne(
        new CreateIndexModel<Seat>(seatIndex, new CreateIndexOptions { Unique = true }));

    // LedgerEntry: Index บน UserId
    // ทำให้ Aggregation SUM เร็วขึ้นมาก (ไม่ต้อง Full Collection Scan)
    var ledgerIndex = Builders<LedgerEntry>.IndexKeys.Ascending(l => l.UserId);
    ledgerCollection.Indexes.CreateOne(new CreateIndexModel<LedgerEntry>(ledgerIndex));
}
```

Index สำคัญมากสำหรับ Production เพราะ:
- `GetByShowtimeAsync` ต้องเจอ Seat หลายร้อยแถวต่อ Showtime
- `GetBalanceAsync` ต้อง SUM ทุก LedgerEntry ของ User

### Password Hashing — BCrypt

```csharp
public class PasswordHasher : IPasswordHasher
{
    // BCrypt ทำงานช้าโดยตั้งใจ (Work Factor)
    // ทำให้ Brute Force Attack ช้ามาก
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
```

BCrypt ดีกว่า MD5/SHA256 เพราะ:
- มี Salt อัตโนมัติ — Hash เดียวกันได้ผลต่างกันทุกครั้ง
- Work Factor ทำให้ยิ่ง Hardware เร็ว ก็ยิ่งปรับ Cost ได้

### JWT Service

```csharp
public string GenerateToken(string userId, string email)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!));

    var token = new JwtSecurityToken(
        claims: [
            new Claim(ClaimTypes.NameIdentifier, userId),  // ← ใช้ดึง userId ใน Controller
            new Claim(ClaimTypes.Email, email)
        ],
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

## 10. Application Layer

### DTO Pattern — ทำไมไม่ส่ง Entity ตรงๆ

```
Entity (Domain)     → มีทุก Property รวมถึงข้อมูลภายใน
DTO (Application)   → ส่งเฉพาะสิ่งที่ Client ต้องการ
```

ตัวอย่าง `Seat` Entity vs `SeatDto`:
```csharp
// Entity — มีทุกอย่าง รวมถึง Version (ไม่ควรส่งให้ Client)
public class Seat
{
    public string Id { get; }
    public string SeatCode { get; }
    public SeatStatus Status { get; }
    public int Version { get; }      // ← ไม่ควรเปิดเผย
}

// DTO — ส่งเฉพาะที่ต้องการ
public record SeatDto(
    string Id,
    string SeatCode,
    SeatType Type,
    decimal Price,
    SeatStatus Status    // ← Client ใช้แสดงสี Available/Locked/Booked
);
```

### FluentValidation — Validator แต่ละตัว

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
2. **Domain Validation** (Domain Layer) — ตรวจ Business Rules

---

## 11. WebAPI Layer

### [Authorize] — ป้องกัน Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]                  // ← ทุก Endpoint ใน Controller นี้ต้องมี JWT Token
public class BookingsController : ControllerBase
```

### ดึง UserId จาก JWT Token

```csharp
// ไม่รับ UserId จาก Request Body
// เพราะ User อาจส่ง UserId ของคนอื่นมาได้ (Security Risk)
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? throw new UnauthorizedAccessException("User not found in token.");
```

### ExceptionHandlingMiddleware — Error Mapping

```csharp
var (statusCode, message) = exception switch
{
    KeyNotFoundException  => (404, exception.Message),
    ArgumentException     => (400, exception.Message),  // Domain Validation
    UnauthorizedAccessException => (401, exception.Message),
    InvalidOperationException   => (409, exception.Message),  // Conflict (ที่นั่งถูกจอง)
    _                    => (500, "เกิดข้อผิดพลาด กรุณาลองใหม่")
};
```

---

## 12. Infrastructure Diagram — การ Deploy

### Development (Local)

```
┌─────────────────────────────────────────────────────┐
│                   Developer Machine                  │
│                                                      │
│  ┌──────────────────┐    ┌──────────────────────┐   │
│  │   .NET WebAPI    │───▶│  Docker Compose       │   │
│  │   (Port 5000)    │    │                       │   │
│  └──────────────────┘    │  ┌─────────────────┐ │   │
│                           │  │ MongoDB Replica  │ │   │
│  .env                     │  │ Set (Port 27017) │ │   │
│  MONGO_USERNAME=...       │  └─────────────────┘ │   │
│  MONGO_PASSWORD=...       │                       │   │
│  MONGO_PORT=27017         │  ┌─────────────────┐ │   │
│  MONGO_DATABASE=...       │  │     Redis        │ │   │
│  JWT_SECRET=...           │  │   (Port 6379)    │ │   │
│  REDIS_CONNECTION=...     │  └─────────────────┘ │   │
│                           └──────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

**docker-compose.yml สำหรับ Development:**
```yaml
version: '3.8'
services:
  mongo:
    image: mongo:7
    # MongoDB ต้องเป็น Replica Set เพื่อใช้ Transaction ได้
    command: mongod --replSet rs0
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: password
    volumes:
      - mongo_data:/data/db

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  mongo_data:
```

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
    │  Redis Cluster   │       │  MongoDB Atlas        │
    │  (Distributed    │       │  (Managed Replica Set │
    │   Locking)       │       │   Primary + 2 Secondary│
    │                  │       │   Multi-AZ)           │
    │  Master          │       │                       │
    │  Replica x2      │       │  Primary    Secondary │
    └──────────────────┘       │     │          │      │
                               │     ▼          ▼      │
                               │  [DB Data]  [Backup]  │
                               └──────────────────────┘
```

### ทำไมต้องใช้ Redis Cluster แทน Single Redis

```
Single Redis:
├── ถ้า Redis ตาย → ระบบจองพัง
└── Lock หายทั้งหมด

Redis Cluster (Master + Replicas):
├── Master รับ Write (Lock)
├── Replicas รับ Read
└── ถ้า Master ตาย → Replica เป็น Master แทน (Auto Failover)
```

### ทำไมต้องใช้ MongoDB Replica Set

```
Standalone MongoDB:
└── MongoDB Transaction ใช้ไม่ได้เลย!

Replica Set (Primary + 2 Secondary):
├── Transaction ใช้งานได้
├── ถ้า Primary ตาย → Secondary เป็น Primary (Auto Failover)
└── Read จาก Secondary ได้ (ลด Load บน Primary)
```

---

### Kubernetes Deployment (Enterprise Level)

```
┌─────────────────────────────────────────────────────────────────┐
│                      Kubernetes Cluster                          │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    Namespace: production                  │   │
│  │                                                           │   │
│  │  ┌─────────────────────────────────────────┐            │   │
│  │  │              Deployment: webapi           │            │   │
│  │  │  Replicas: 3                              │            │   │
│  │  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ │            │   │
│  │  │  │  Pod 1   │ │  Pod 2   │ │  Pod 3   │ │            │   │
│  │  │  │ .NET API │ │ .NET API │ │ .NET API │ │            │   │
│  │  │  └──────────┘ └──────────┘ └──────────┘ │            │   │
│  │  └─────────────────────────────────────────┘            │   │
│  │                         │                                │   │
│  │            ┌────────────┼────────────┐                  │   │
│  │            ▼            ▼            ▼                   │   │
│  │  ┌──────────────┐  ┌──────────────────────┐            │   │
│  │  │ Redis        │  │ MongoDB StatefulSet   │            │   │
│  │  │ StatefulSet  │  │ Primary + 2 Secondary │            │   │
│  │  └──────────────┘  └──────────────────────┘            │   │
│  │                                                           │   │
│  │  ┌──────────────────────────────────────────┐           │   │
│  │  │  ConfigMap: appsettings                  │           │   │
│  │  │  Secret: JWT_SECRET, MONGO_PASSWORD      │           │   │
│  │  └──────────────────────────────────────────┘           │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

**Horizontal Scaling ทำงานอย่างไร:**

ด้วย Redis Distributed Lock ระบบ Scale ได้โดยไม่มีปัญหา Race Condition:
```
User A → Pod 1 → ขอ Redis Lock seat-lock:A1 → ✅ ได้
User B → Pod 2 → ขอ Redis Lock seat-lock:A1 → ❌ ไม่ได้ (Pod ไหนก็ไม่ได้)
User C → Pod 3 → ขอ Redis Lock seat-lock:A1 → ❌ ไม่ได้

Lock อยู่ที่ Redis (Shared) ไม่ใช่ Memory ของแต่ละ Pod
```

---

## 13. สรุป Engineering Decisions

### Design Patterns ที่ใช้

| Pattern | ใช้ที่ไหน | แก้ปัญหาอะไร |
|---|---|---|
| **Append-Only Ledger** | Wallet / LedgerEntry | Lost Update บน Balance, Audit Trail |
| **Optimistic Concurrency Control** | Seat Booking | Concurrent Update บน Document เดียวกัน |
| **Distributed Lock** | Redis | Race Condition ระหว่าง Application Instances |
| **MongoDB Transaction** | Booking Flow | Partial Failure — All-or-Nothing |
| **Static Factory Method** | ทุก Entity | Validation ก่อนสร้าง Object |
| **Repository Pattern** | Infrastructure | แยก DB Logic ออกจาก Business Logic |
| **Middleware Pipeline** | Exception Handling | จัดการ Error กลางที่เดียว |
| **DTO Pattern** | Application Layer | ซ่อน Internal Fields (Version, etc.) |

### ลำดับการป้องกัน Race Condition

```
Layer 1: Redis Distributed Lock
└── ป้องกันไม่ให้หลาย Request เข้าสู่ Booking Flow พร้อมกัน

Layer 2: OCC Version Check
└── ป้องกัน Lost Update ระดับ Database (Defense in Depth)

Layer 3: MongoDB Transaction
└── ป้องกัน Partial Failure — Rollback ทุกอย่างถ้ามีอะไรผิดพลาด

3 Layer ทำงานร่วมกัน → ระบบปลอดภัยแม้มี Load สูง
```

### Environment Variables ที่ต้องตั้งค่า

```env
# MongoDB
MONGO_USERNAME=admin
MONGO_PASSWORD=your-secure-password
MONGO_PORT=27017
MONGO_DATABASE=movieticket

# Redis
REDIS_CONNECTION=localhost:6379

# JWT
JWT_SECRET=your-super-secret-key-minimum-32-characters
```

### Packages ที่ใช้

| Package | ใช้ทำอะไร |
|---|---|
| `MongoDB.Driver` | ติดต่อ MongoDB |
| `StackExchange.Redis` | Redis Distributed Lock |
| `BCrypt.Net-Next` | Password Hashing |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT Authentication |
| `FluentValidation.AspNetCore` | Input Validation |
| `DotNetEnv` | โหลด .env file |


# movie-ticket-booking-system

# สร้าง migrations ครั้งแรก
dotnet ef migrations add InitialCreate

# อัปเดต Database (สร้างตาราง)
dotnet ef database update

# เพิ่ม column หรือแก้ไข Entity แล้วต้องการอัปเดต
dotnet ef migrations add AddColumnToMovie
dotnet ef database update

# ย้อนกลับ migration ล่าสุด
dotnet ef migrations remove

# ดู migration ทั้งหมด
dotnet ef migrations list

# ลบ database แล้วสร้างใหม่
dotnet ef database drop
dotnet ef database update