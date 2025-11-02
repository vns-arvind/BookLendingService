using BookLending.Application.DTOs;

namespace BookLending.Application.Interfaces;

public interface IBookService
{
    Task<BookDto> AddBookAsync(CreateBookDto dto, CancellationToken ct = default);
    Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken ct = default);
    Task<bool> CheckoutAsync(Guid id, CancellationToken ct = default);
    Task<bool> ReturnAsync(Guid id, CancellationToken ct = default);
}
