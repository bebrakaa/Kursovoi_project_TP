namespace MicroserviceReader.Models;
public class Reader
{
    public long Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public int BorrowedBooks { get; set; }
}
