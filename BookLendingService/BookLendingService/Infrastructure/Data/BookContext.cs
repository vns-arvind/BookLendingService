using BookLending.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLending.Infrastructure.Data;

public class BookContext : DbContext
{
    public BookContext(DbContextOptions<BookContext> opts) : base(opts) { }
    public DbSet<Book> Books { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(500);
        });
    }
}
