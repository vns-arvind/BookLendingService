namespace BookLending.Application.DTOs;

public record CreateBookDto(string Title, string? Author);
public record BookDto(Guid Id, string Title, string? Author, bool IsAvailable, DateTime CreatedAt);
