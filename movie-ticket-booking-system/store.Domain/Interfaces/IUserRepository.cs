using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username); // ใช้ตอน Login — หา User จาก Username
    Task AddAsync(User user);                        // ใช้ตอน Register — บันทึก User ใหม่
}