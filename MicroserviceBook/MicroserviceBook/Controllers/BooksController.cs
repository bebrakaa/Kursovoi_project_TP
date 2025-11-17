using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroserviceBook.Models;

namespace MicroserviceBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(long id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return book;
        }

        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(long id, Book book)
        {
            if (id != book.Id) return BadRequest();
            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(long id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
