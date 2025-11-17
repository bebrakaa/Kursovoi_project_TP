using System.Text.Json.Serialization;

namespace MicroserviceCompositeLibrary.Contracts;

public class Book
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("IsAvailable")]
    public bool IsAvailable { get; set; }
}
