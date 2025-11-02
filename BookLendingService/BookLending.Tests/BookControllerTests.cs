using BookLending.Api.Controllers;
using BookLending.Application.DTOs;
using BookLending.Application.Interfaces;
using BookLending.Domain.Entities;
using BookLending.Infrastructure.Repositories;
using BookLending.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLending.Tests
{
    public class BookControllerTests
    {
        private readonly Mock<IBookService> _bookService;
        private readonly Mock<IValidator<CreateBookDto>> _validator;
        private readonly Mock<ILogger<BookController>> _logger;
        private readonly BookController _controller;

        public BookControllerTests()
        {
            _bookService = new Mock<IBookService>();
            _validator = new Mock<IValidator<CreateBookDto>>();
            _logger = new Mock<ILogger<BookController>>();
            _controller = new BookController(_bookService.Object, _validator.Object, _logger.Object);
        }

        [Fact]
        public async Task Add_ValidBook_Should_Returns_CreatedResult()
        {
            // Arrange

            var createBookDto = new CreateBookDto
            (
                "Test Book",
                "Test Author"
            );            

            _validator.Setup(v => v.ValidateAsync(createBookDto, default)).ReturnsAsync(new ValidationResult());

            _bookService.Setup(s => s.AddBookAsync(createBookDto, default)).ReturnsAsync(new BookDto
            (
                Guid.NewGuid(),
                createBookDto.Title,
                createBookDto.Author,
                true,
                DateTime.UtcNow
            ));

            var add = await _controller.Add(createBookDto);

            Assert.IsType<CreatedResult>(add);
            Assert.NotNull(((CreatedResult)add).Value);
            Assert.Equal(201, ((CreatedResult)add).StatusCode);
            Assert.StartsWith("/books/", ((CreatedResult)add).Location);
            Assert.IsType<BookDto>(((CreatedResult)add).Value);
            Assert.Equal(createBookDto.Title, ((BookDto)((CreatedResult)add).Value).Title); 
            Assert.Equal(createBookDto.Author, ((BookDto)((CreatedResult)add).Value).Author);

            //Assert.IsType<CreatedResult>(await add);
        }

        [Fact]
        public async Task Add_InvalidBook_Should_Return_BadRequest()
        {
            // Arrange

            var createBookDto = new CreateBookDto("Title", "Author");
            var failure = new ValidationFailure("Title", "Incorrect Title");
            var lstFailures = new List<ValidationFailure>();
            lstFailures.Add(failure);
            var validationResult = new ValidationResult(lstFailures);
            _validator.Setup(r=> r.ValidateAsync(createBookDto, default)).ReturnsAsync(validationResult);

            // Act

            var result = await _controller.Add(createBookDto);

            // Assert

            Assert.IsType<BadRequestObjectResult>(result);

        }

        [Fact]
        public async Task GetAll_Should_Returns_OkResult_With_All_Books()
        {
            var bookDto = new BookDto(new Guid(), "Title", "Author", true, DateTime.Now);
            var lstBook = new List<BookDto>();
            lstBook.Add(bookDto);

            _bookService.Setup(r=>r.GetAllBooksAsync(default)).ReturnsAsync(lstBook);

            var result = await _controller.GetAll();
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
