using Store.Application.DTOs;

namespace Store.Application.Interfaces;

public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllAsync();
    Task<MovieDto?> GetByIdAsync(string id);
    Task<MovieDto> CreateAsync(CreateMovieDto dto);
}
