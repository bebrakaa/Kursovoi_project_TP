using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroserviceReader.Models;

namespace MicroserviceReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadersController : ControllerBase
    {
        private readonly ReaderContext _context;

        public ReadersController(ReaderContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reader>>> GetReaders()
        {
            return await _context.Readers.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reader>> GetReader(long id)
        {
            var reader = await _context.Readers.FindAsync(id);
            if (reader == null)
                return NotFound();
            return reader;
        }

        [HttpPost]
        public async Task<ActionResult<Reader>> PostReader(Reader reader)
        {
            _context.Readers.Add(reader);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReader), new { id = reader.Id }, reader);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReader(long id, Reader reader)
        {
            if (id != reader.Id)
                return BadRequest();
            _context.Entry(reader).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReader(long id)
        {
            var reader = await _context.Readers.FindAsync(id);
            if (reader == null)
                return NotFound();
            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
