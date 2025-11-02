using BookLending.Application.DTOs;
using BookLending.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BookLending.Api.Controllers;

[ApiController]
[Route("books")]
public class BookController : ControllerBase
{
    private readonly IBookService _svc;
    private readonly IValidator<CreateBookDto> _validator;
    private readonly ILogger<BookController> _logger;

    public BookController(IBookService svc, IValidator<CreateBookDto> validator, ILogger<BookController> logger)
    {
        _svc = svc;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateBookDto dto)
    {
        // Validate input using FluentValidation
        var validationResult = await _validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            var problem = new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
                )
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed for Add book endpoint",
                Detail = "One or more validation errors occurred."
            };

            return BadRequest(problem);
        }

        var created = await _svc.AddBookAsync(dto);
        return Created($"/books/{created.Id}", created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _svc.GetAllBooksAsync();
        return Ok(list);
    }

    [HttpPost("{id:guid}/checkout")]
    public async Task<IActionResult> Checkout(Guid id)
    {
        var ok = await _svc.CheckoutAsync(id);
        if (!ok)
        {
            return Problem(
                title: "Book not found",
                detail: $"The book with ID {id} could not be checked out.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id)
    {
        var ok = await _svc.ReturnAsync(id);
        if (!ok)
        {
            return Problem(
                title: "Invalid return",
                detail: $"The book with ID {id} is already available or does not exist.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return NoContent();
    }
}
