using Microsoft.EntityFrameworkCore;
using MicroserviceBook.Models;

namespace MicroserviceBook;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
}
