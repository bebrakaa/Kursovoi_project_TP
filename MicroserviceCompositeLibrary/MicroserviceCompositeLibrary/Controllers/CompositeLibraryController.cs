using Microsoft.AspNetCore.Mvc;
using MicroserviceCompositeLibrary.Models;
using MicroserviceCompositeLibrary.Contracts;
using System.Text.Json;

namespace MicroserviceCompositeLibrary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompositeLibraryController : ControllerBase
{
    private readonly string _booksApi = "https://localhost:7202/api/books";
    private readonly string _readersApi = "https://localhost:7279/api/readers";

    [HttpGet("library-info")]
    public async Task<ActionResult<List<BorrowInfo>>> GetLibraryInfo()
    {
        HttpClientHandler handler = new()
        {
            ServerCertificateCustomValidationCallback = (s, c, ch, e) => true
        };
        using var client = new HttpClient(handler);

        // --- запросы к Book API и Reader API ---
        var booksResponse = await client.GetAsync(_booksApi);
        if (!booksResponse.IsSuccessStatusCode)
            return StatusCode(500, "Books API unavailable");

        var readersResponse = await client.GetAsync(_readersApi);
        if (!readersResponse.IsSuccessStatusCode)
            return StatusCode(500, "Readers API unavailable");

        // --- десериализация ---
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var books = await JsonSerializer.DeserializeAsync<List<Book>>(
            await booksResponse.Content.ReadAsStreamAsync(), options);
        var readers = await JsonSerializer.DeserializeAsync<List<Reader>>(
            await readersResponse.Content.ReadAsStreamAsync(), options);

        if (books is null || readers is null)
            return StatusCode(500, "Failed to deserialize external data");

        // --- логика L1 (лимит — не более 3 книг в руки) ---
        int availableBooks = books.Count(b => b.IsAvailable);
        const int maxIssueLimit = 3;

        var result = readers.Select(r =>
        {
            int remainingQuota = Math.Max(0, maxIssueLimit - r.BorrowedBooks);
            int canIssue = Math.Min(availableBooks, remainingQuota);

            return new BorrowInfo
            {
                ReaderName = r.FullName,
                BorrowedCount = r.BorrowedBooks,
                BookCountAvailable = canIssue
            };
        }).ToList();

        return Ok(result);
    }
}
