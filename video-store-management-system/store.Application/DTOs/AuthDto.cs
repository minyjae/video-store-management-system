namespace store.Application.DTOs;

public record RegisterDto(
    string Username,
    string Password
);

public record LoginDto(
    string Username,
    string Password
);