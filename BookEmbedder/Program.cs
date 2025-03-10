using System.Text.Json;
using BookEmbedder;
using BookEmbedder.Models;

Console.WriteLine("Loading book dataset...");

// Path to the JSON file
string jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "books.json");

// Configure JSON options
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    Converters = { new BookJsonConverter() } // Register the custom converter
};

// Read JSON file
string jsonString = File.ReadAllText(jsonFilePath);
List<Book> books = JsonSerializer.Deserialize<List<Book>>(jsonString, options) ?? new();

Console.WriteLine($"Loaded {books.Count} books.");

// Display first few books for verification
foreach (var book in books.Take(5))
{
    Console.WriteLine($"Title: {book.Title}");
    Console.WriteLine($"Summary: {book.Summary[..Math.Min(book.Summary.Length, 100)]}...");
    Console.WriteLine("----");
}

// Book record
public record Book(int Index, string Title, string Genre, string Summary);
