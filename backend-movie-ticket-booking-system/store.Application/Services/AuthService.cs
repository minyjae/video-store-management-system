using store.Application.Interfaces;
using store.Domain.Interfaces;
using store.Domain.Entities;

namespace store.Application.Services;
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

    public async Task<string> RegisterAsync(string username, string password)
    {
        var hashedPassword = _passwordHasher.Hash(password);  // Hash ที่นี่
        var user = User.Register(username, hashedPassword);      // ส่ง hash เข้า Entity
        await _userRepository.AddAsync(user);
        return _jwtService.GenerateToken(user.Id, user.Username, user.Role);
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null)
            throw new KeyNotFoundException("User not found.");

        if (!_passwordHasher.Verify(password, user.HashedPassword))
            throw new ArgumentException("Your username or password are wrong.");

        return _jwtService.GenerateToken(user.Id, user.Username, user.Role);
    }
}