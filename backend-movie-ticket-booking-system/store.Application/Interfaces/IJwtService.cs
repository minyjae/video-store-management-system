using store.Domain.Enums;

namespace store.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username, UserRole role);
}