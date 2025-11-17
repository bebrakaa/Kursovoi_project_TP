using System.Text.Json.Serialization;

namespace MicroserviceCompositeLibrary.Models;

public class BorrowInfo
{
    [JsonPropertyName("readerName")]
    public string ReaderName { get; set; }

    [JsonPropertyName("borrowedCount")]
    public int BorrowedCount { get; set; }

    [JsonPropertyName("bookCountAvailable")]
    public int BookCountAvailable { get; set; }
}
