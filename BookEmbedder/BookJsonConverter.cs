using Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookEmbedder;

public class BookJsonConverter : JsonConverter<Book>
{
    public override Book Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;

            // Extract values with fallback defaults
            string title = root.GetProperty("title").ToString(); // Convert everything to string
            string genre = root.GetProperty("genre").GetString() ?? "Unknown";
            string summary = root.GetProperty("summary").GetString() ?? "";

            return new Book(title, genre, summary);
        }
    }

    public override void Write(Utf8JsonWriter writer, Book value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            title = value.Title,
            genre = value.Genre,
            summary = value.Summary
        }, options);
    }
}
