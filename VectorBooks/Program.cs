using Npgsql;
using Microsoft.Extensions.Configuration;
using Shared.Models;
using Shared.Ollama;

Console.WriteLine("Starting book recommendation system...");

// Load configuration
IConfigurationRoot config = new ConfigurationBuilder()
                                .AddUserSecrets<Program>()
                                .Build();

// Database connection string
string connectionString = config["ConnectionStrings:Postgres"]!;

// Get user input
Console.Write("Provide input: ");
string query = Console.ReadLine()?.Trim() ?? "";

if (string.IsNullOrEmpty(query))
{
    Console.WriteLine("No input provided. Exiting.");
    return;
}

// Initialize HTTP client (reused)
using HttpClient httpClient = new();
using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

// Get embedding for user input
var embedding = await OllamaGateway.GetEmbeddingAsync(httpClient, query);

if (embedding.Length == 0)
{
    Console.WriteLine("Failed to retrieve embedding. Exiting.");
    return;
}

// Retrieve recommended books
var recommendations = await GetRecommendedBooksAsync(conn, embedding);

// Display recommendations
if (recommendations.Count == 0)
{
    Console.WriteLine("No similar books found.");
}
else
{
    Console.WriteLine("Recommended Books:");
    foreach (var book in recommendations)
    {
        Console.WriteLine($"Title: {book.Title}");
        Console.WriteLine($"Summary: {book.Summary[..Math.Min(200, book.Summary.Length)]}..."); // Truncate long summaries
        Console.WriteLine("----");
    }
}

// Retrieve recommended books based on query embedding
async Task<List<Book>> GetRecommendedBooksAsync(NpgsqlConnection conn, float[] queryEmbedding)
{
    string sql = @"
        SELECT id, title, summary, genre, embedding <=> @query_embedding::vector(768) AS distance
        FROM books
        ORDER BY distance
        LIMIT 5;
    ";

    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("query_embedding", queryEmbedding);

    var recommendedBooks = new List<Book>();

    using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            var title = reader.GetString(1);
            var summary = reader.GetString(2);
            var genre = reader.GetString(3);
            recommendedBooks.Add(new Book(title, genre, summary));
        }
    }

    return recommendedBooks;
}
