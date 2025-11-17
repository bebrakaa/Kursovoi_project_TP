using System.Text.Json.Serialization;

namespace MicroserviceCompositeLibrary.Contracts;
public class Reader
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("fullName")] 
    public string FullName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("borrowedBooks")]
    public int BorrowedBooks { get; set; }
}
