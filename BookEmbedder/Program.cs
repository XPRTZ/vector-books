using System.Text.Json;
using Npgsql;
using BookEmbedder;
using Microsoft.Extensions.Configuration;
using Shared.Models;
using Shared.Ollama;

Console.WriteLine("Loading book dataset...");

// Loading configuration
IConfigurationRoot config = new ConfigurationBuilder()
                                .AddUserSecrets<Program>()
                                .Build();

// Database connection string
string connectionString = config["ConnectionStrings:Postgres"]!;

// Path to JSON file
string jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "books.json");

// Configure JSON options
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new BookJsonConverter() } };

// Read JSON file
string jsonString = File.ReadAllText(jsonFilePath);
List<Book> books = JsonSerializer.Deserialize<List<Book>>(jsonString, options) ?? new();

Console.WriteLine($"Loaded {books.Count} books.");

// Initialize HTTP client
using HttpClient httpClient = new();
using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

// Insert books into PostgreSQL
using var cmd = new NpgsqlCommand("INSERT INTO books (title, summary, genre, embedding) VALUES (@title, @summary, @genre, @embedding)", conn);

foreach (var book in books)
{
    var embedding = await OllamaGateway.GetEmbeddingAsync(book.Summary);

    cmd.Parameters.Clear();
    cmd.Parameters.AddWithValue("title", book.Title);
    cmd.Parameters.AddWithValue("summary", book.Summary);
    cmd.Parameters.AddWithValue("genre", book.Genre);
    cmd.Parameters.AddWithValue("embedding", embedding);

    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"Inserted: {book.Title}");
}        
