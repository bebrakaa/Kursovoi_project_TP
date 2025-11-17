using Microsoft.EntityFrameworkCore;
using MicroserviceReader.Models;

namespace MicroserviceReader.Models
{
    public class ReaderContext : DbContext
    {
        public ReaderContext(DbContextOptions<ReaderContext> options) : base(options) { }

        public DbSet<Reader> Readers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reader>(entity =>
            {
                entity.ToTable("readers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.BorrowedBooks).HasColumnName("borrowed_books");
            });
        }
    }
}
