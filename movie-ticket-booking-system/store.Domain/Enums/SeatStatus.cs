namespace store.Domain.Enums;

public enum SeatStatus
{
    Available,  // ว่าง — จองได้
    Locked,     // ถูกล็อคชั่วคราว (Redis Lock กำลังทำงาน)
    Booked      // จองสำเร็จแล้ว — ห้ามแตะ
}

public enum SeatType
{
    Normal,  // ราคาปกติ
    VIP      // ราคาพิเศษ
}

public enum LedgerEntryType
{
    Deposit,         // เติมเงิน
    TicketPurchase   // ซื้อตั๋ว (ติดลบ)
}