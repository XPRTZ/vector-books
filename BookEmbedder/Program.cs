using System.Net.Http;
using System.Text;
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
    Converters = { new BookJsonConverter() } // Ensure case-insensitive deserialization
};

// Read and parse JSON
string jsonString = File.ReadAllText(jsonFilePath);
List<Book> books = JsonSerializer.Deserialize<List<Book>>(jsonString, options) ?? new();

Console.WriteLine($"Loaded {books.Count} books.");

// Initialize HTTP client for Ollama API
using HttpClient httpClient = new();

foreach (var book in books.Take(5)) // Limit to 5 for testing
{
    string embedding = await GetEmbeddingAsync(httpClient, book.Summary);
    Console.WriteLine($"Title: {book.Title}");
    Console.WriteLine($"Embedding: {embedding[..50]}..."); // Truncate display
    Console.WriteLine("----");
}

// Function to call Ollama API and get embeddings
async Task<string> GetEmbeddingAsync(HttpClient client, string text)
{
    string url = "http://localhost:11434/api/embeddings";
    var requestData = new { model = "nomic-embed-text", prompt = text };
    string jsonRequest = JsonSerializer.Serialize(requestData);
    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

    HttpResponseMessage response = await client.PostAsync(url, content);
    response.EnsureSuccessStatusCode();

    string jsonResponse = await response.Content.ReadAsStringAsync();
    var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse, options);

    return embeddingResponse?.Embedding is not null
        ? string.Join(",", embeddingResponse.Embedding)
        : "No embedding generated.";
}

// Model for embedding response
record EmbeddingResponse(float[] Embedding);
