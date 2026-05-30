using store.Application.DTOs;

namespace store.Application.Interfaces;

public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllAsync();
    Task<MovieDto?> GetByIdAsync(Guid id);
    Task<MovieDto> CreateAsync(CreateMovieDto dto, IFormFile poster, string webRootPath);
    Task<MovieDto?> UpdateAsync(UpdateMovieDto dto);
    Task<MovieDto> UploadPosterAsync(Guid movieId, IFormFile file, string webRootPath);
}
