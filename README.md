# video-store-management-system

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