using Shared.Models;
using System.Text;
using System.Text.Json;

namespace Shared.Ollama
{
    public static class OllamaGateway
    {
        // Function to call Ollama API for embeddings
        public async static Task<float[]> GetEmbeddingAsync(HttpClient client, string text)
        {
            string url = "http://localhost:11434/api/embeddings";
            var requestData = new { model = "nomic-embed-text", prompt = text };
            string jsonRequest = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (embeddingResponse?.Embedding == null || embeddingResponse.Embedding.Length == 0)
            {
                Console.WriteLine("Error: Received empty embedding response.");
                return Array.Empty<float>();
            }

            return embeddingResponse?.Embedding ?? Array.Empty<float>();
        }
    }
}
