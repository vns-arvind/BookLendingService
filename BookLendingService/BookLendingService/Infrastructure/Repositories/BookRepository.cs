using BookLending.Domain.Entities;
using BookLending.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookLending.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly BookContext _ctx;
        public BookRepository(BookContext ctx) => _ctx = ctx;

        public async Task AddAsync(Book book, CancellationToken ct = default)
        {
            await _ctx.Books.AddAsync(book, ct);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _ctx.Books.FirstOrDefaultAsync(b => b.Id == id, ct);

        public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default) =>
            await _ctx.Books.OrderBy(b => b.Title).ToListAsync(ct);

        public async Task UpdateAsync(Book book, CancellationToken ct = default)
        {
            _ctx.Books.Update(book);
            await _ctx.SaveChangesAsync(ct);
        }
    }
}
