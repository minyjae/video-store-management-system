using store.Application.DTOs;

namespace store.Application.Interfaces;

public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllAsync();
    Task<MovieDto?> GetByIdAsync(string id);
    Task<MovieDto> CreateAsync(CreateMovieDto dto);
    Task<MovieDto?> UpdateAsync(UpdateMovieDto dto);
}
