namespace store.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(string username, string password); // คืน JWT Token
    Task<string> LoginAsync(string username, string password);    // คืน JWT Token
}