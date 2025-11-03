using BookLending.Application.DTOs;
using BookLending.Domain.Entities;
using BookLending.Infrastructure.Repositories;
using BookLending.Services;
using BookLending.Validators;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookLending.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly IMemoryCache _memoryCache;
        private readonly BookService _service;
        private readonly Mock<ILogger<BookService>> _loggerMock;
        private Guid _bookGuid;

        public BookServiceTests()
        {
            _bookGuid = Guid.NewGuid();
            _bookRepoMock = new Mock<IBookRepository>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<BookService>>();
            _service = new BookService(_bookRepoMock.Object, _memoryCache, _loggerMock.Object);
        }

        [Fact]
        public async Task AddBookAsync_Should_Add_Book_Successfully()
        {
            // Arrange
            var newBook = new CreateBookDto ( "Test Book", "Tester" );

            _bookRepoMock.Setup(r => r.AddAsync(It.IsAny<Book>(), default))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddBookAsync(newBook, default);

            // Assert
            _bookRepoMock.Verify(r => r.AddAsync(It.IsAny<Book>(), default));
            Assert.Equal("Test Book", result.Title);
        }

        [Fact]
        public async Task AddBookAsync_Should_Invalidate_Cache()
        {
            // Arrange
            var newBook = new CreateBookDto ( "Test Book", "Tester" );
            _bookRepoMock.Setup(r => r.AddAsync(It.IsAny<Book>(), default))
                         .Returns(Task.CompletedTask);
            // Pre-populate cache
            var cachedBooks = new List<BookDto>
            {
                new BookDto ( Guid.NewGuid(), "Cached Book", "Cached Author", true, DateTime.Now )
            };
            _memoryCache.Set("all_books", cachedBooks);
            // Act
            await _service.AddBookAsync(newBook, default);
            // Assert
            Assert.False(_memoryCache.TryGetValue("all_books", out _));
        }

        /*
         * 
         * There is no test for validation failure in AddBookAsync because the validation is typically handled globally
         * to keep controllers & service clean.
         * 
         * Global validation can be handled through integration testing of the API endpoints or middleware.
         * 
         * Testing validor separately as below.
         * 
         */

        [Fact]
        public void Validator_Should_Fail_When_Title_Empty()
        {
            var validator = new CreateBookDtoValidator();
            var dto = new CreateBookDto("", "");

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        }

        [Fact]
        public async Task GetAllBooksAsync_Should_Return_Books()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = _bookGuid, Title = "Book 1", Author = "Author 1" },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2" }
            };
            _bookRepoMock.Setup(r => r.GetAllAsync(default))
                         .ReturnsAsync(books);
            // Act
            var result = await _service.GetAllBooksAsync(default);
            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, b => b.Title == "Book 1");
            Assert.Contains(result, b => b.Title == "Book 2");
        }

        [Fact]
        public async Task GetAllBooksAsync_Should_Return_Empty_List_When_No_Books()
        {
            // Arrange
            _bookRepoMock.Setup(r => r.GetAllAsync(default))
                         .ReturnsAsync(new List<Book>());
            // Act
            var result = await _service.GetAllBooksAsync(default);
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBooksAsync_Should_Return_Books_From_Cache_When_Available()
        {
            // Arrange
            var cachedBooks = new List<BookDto>
            {
                new BookDto ( _bookGuid, "Cached Book", "Cached Author", true, DateTime.Now )
            };

            _memoryCache.Set("all_books", cachedBooks);

            // Act
            var result = await _service.GetAllBooksAsync(default);

            // Assert
            _bookRepoMock.Verify(r => r.GetAllAsync(default), Times.Never);
            Assert.Single(result);
            Assert.Equal("Cached Book", result.First().Title);            
        }

        [Fact]
        public async Task GetBooksAsync_Should_Fetch_From_Repo_If_Not_In_Cache()
        {
            // Arrange
            var repoBooks = new List<Book>
            {
                new Book { Id = _bookGuid, Title = "Repo Book", Author = "Author" }
            };

            _bookRepoMock.Setup(r => r.GetAllAsync(default))
                         .ReturnsAsync(repoBooks);

            // Act
            var result = await _service.GetAllBooksAsync(default);

            // Assert
            _bookRepoMock.Verify(r => r.GetAllAsync(default), Times.Once);
            Assert.Single(result);
            Assert.Equal("Repo Book", result.First().Title);            
        }

        [Fact]
        public async Task CheckoutBookAsync_Should_Mark_Book_Unavailable()
        {
            // Arrange
            var book = new Book { Id = _bookGuid, Title = "Book 1", IsAvailable = true };
            _bookRepoMock.Setup(r => r.GetByIdAsync(_bookGuid, default)).ReturnsAsync(book);
            _bookRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Book>(), default)).Returns(Task.CompletedTask);

            // Act
            await _service.CheckoutAsync(_bookGuid, default);

            // Assert

            _bookRepoMock.Verify(r => r.UpdateAsync(book, default), Times.Once);
            Assert.False(book.IsAvailable);
        }

        [Fact]
        public async Task ReturnBookAsync_Should_Mark_Book_Available()
        {
            // Arrange
            var book = new Book { Id = _bookGuid, Title = "Book 1", IsAvailable = false };
            _bookRepoMock.Setup(r => r.GetByIdAsync(_bookGuid, default)).ReturnsAsync(book);
            _bookRepoMock.Setup(r => r.UpdateAsync(book, default)).Returns(Task.CompletedTask);

            // Act
            await _service.ReturnAsync(_bookGuid);

            // Assert
            _bookRepoMock.Verify(r => r.UpdateAsync(book, default), Times.Once);
            Assert.True(book.IsAvailable);            
        }

        [Fact]
        public async Task CheckoutBookAsync_Should_Throw_When_Book_Not_Found()
        {
            // Arrange
            _bookRepoMock.Setup(r => r.GetByIdAsync(_bookGuid, default)).ReturnsAsync((Book?)null);

            var result = await _service.CheckoutAsync(_bookGuid);

            // Act & Assert
            Assert.False(result);
        }
    }
}
