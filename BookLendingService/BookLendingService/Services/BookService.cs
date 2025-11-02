using BookLending.Application.DTOs;
using BookLending.Application.Interfaces;
using BookLending.Domain.Entities;
using BookLending.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace BookLending.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _repo;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BookService> _logger;
    private static readonly string AllBooksCacheKey = "all_books";
    public BookService(IBookRepository repo, IMemoryCache cache, ILogger<BookService> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BookDto> AddBookAsync(CreateBookDto dto, CancellationToken ct = default)
    {
        var book = new Book { Title = dto.Title, Author = dto.Author };
        await _repo.AddAsync(book, ct);
        _cache.Remove(AllBooksCacheKey); // invalidate cache
        return Map(book);
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken ct = default)
    {
        if (!_cache.TryGetValue(AllBooksCacheKey, out IEnumerable<BookDto> books))
        {
            books = (await _repo.GetAllAsync(ct)).Select(Map);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60));

            _cache.Set(AllBooksCacheKey, books, cacheOptions);
            _logger.LogInformation("Books cache refreshed");
        }

        return books;
    }

    public async Task<bool> CheckoutAsync(Guid id, CancellationToken ct = default)
    {
        var book = await _repo.GetByIdAsync(id, ct);
        if (book == null || !book.IsAvailable) return false;
        book.IsAvailable = false;
        await _repo.UpdateAsync(book, ct);
        _cache.Remove(AllBooksCacheKey);
        return true;
    }

    public async Task<bool> ReturnAsync(Guid id, CancellationToken ct = default)
    {
        var book = await _repo.GetByIdAsync(id, ct);
        if (book == null || book.IsAvailable) return false;
        book.IsAvailable = true;
        await _repo.UpdateAsync(book, ct);
        _cache.Remove(AllBooksCacheKey);
        return true;
    }

    private static BookDto Map(Book b) => new(b.Id, b.Title, b.Author, b.IsAvailable, b.CreatedAt);
}
