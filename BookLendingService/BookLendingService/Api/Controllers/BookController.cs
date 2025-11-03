using BookLending.Application.DTOs;
using BookLending.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IBookService _service;
    private readonly ILogger<BookController> _logger;

    public BookController(IBookService service, ILogger<BookController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateBookDto dto)
    {
        var created = await _service.AddBookAsync(dto);
        _logger.LogInformation("Book created successfully: {BookId}", created.Id);
        return Ok(created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = await _service.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpPost("{id:guid}/checkout")]
    public async Task<IActionResult> Checkout(Guid id)
    {
        var success = await _service.CheckoutAsync(id);
        if (success)
            return Ok(new { message = $"Book with ID {id} checked out successfully" });

        return Problem(
            title: "Checkout failed",
            detail: $"Book with ID {id} was not found or is already checked out.",
            statusCode: StatusCodes.Status400BadRequest);
    }


    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id)
    {
        var success = await _service.ReturnAsync(id);
        if (success)
            return Ok(new { message = $"Book with ID {id} returned successfully" });

        return Problem(
            title: "Return failed",
            detail: $"Book with ID {id} is already available or does not exist.",
            statusCode: StatusCodes.Status400BadRequest);
    }
}
