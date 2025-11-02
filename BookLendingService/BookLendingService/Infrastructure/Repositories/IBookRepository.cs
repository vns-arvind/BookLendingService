using BookLending.Application.Interfaces;
using BookLending.Domain.Entities;

namespace BookLending.Infrastructure.Repositories
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Book book, CancellationToken ct = default);
        Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(Book book, CancellationToken ct = default);
    }
}
