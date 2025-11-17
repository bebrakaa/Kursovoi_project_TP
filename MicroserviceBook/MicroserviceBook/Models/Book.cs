namespace MicroserviceBook.Models;

public class Book
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }
    public bool IsAvailable { get; set; }
}
