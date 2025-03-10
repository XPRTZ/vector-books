using System.Net.Http;
using System.Text;
using System.Text.Json;
using Npgsql;
using BookEmbedder.Models;
using BookEmbedder;
using System.Globalization;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
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
            string embedding = await GetEmbeddingAsync(httpClient, book.Summary);

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("title", book.Title);
            cmd.Parameters.AddWithValue("summary", book.Summary);
            cmd.Parameters.AddWithValue("genre", book.Genre);
            cmd.Parameters.AddWithValue("embedding", embedding.Split(',').Select(float.Parse).ToArray());

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Inserted: {book.Title}");
        }

        // Function to call Ollama API for embeddings
        async Task<string> GetEmbeddingAsync(HttpClient client, string text)
        {
            string url = "http://localhost:11434/api/embeddings";
            var requestData = new { model = "nomic-embed-text", prompt = text };
            string jsonRequest = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Log the raw response for debugging
            Console.WriteLine($"Raw API response: {jsonResponse}");

            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse, options);

            // Format each float in the embedding to use a period as the decimal separator
            return embeddingResponse?.Embedding is not null
                ? string.Join(",", embeddingResponse.Embedding.Select(e => e.ToString("G", CultureInfo.InvariantCulture)))
                : "";
        }

    }
}

// Model for embedding response
record EmbeddingResponse(float[] Embedding);