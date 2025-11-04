using BusinessLayer.DTOs;

namespace BusinessLayer.Interfaces;

public interface ITitleService
{
    Task<TitleFullDTO?> GetByIdAsync(string tconst, CancellationToken ct = default);

    Task<IReadOnlyList<TitlePreviewDTO>> GetByGenreAsync(
        int genreId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);
}
