using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Shared.Models;
using System.Text;
using System.Text.Json;

namespace Shared.Ollama
{
    public static class OllamaGateway
    {
        public async static Task<float[]> GetEmbeddingAsync(string text)
        {
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            Kernel kernel = Kernel.CreateBuilder()
                .AddOllamaTextEmbeddingGeneration(
                    endpoint: new Uri("http://localhost:11434"),
                    modelId: "nomic-embed-text")
                .Build();
#pragma warning restore SKEXP0070

#pragma warning disable SKEXP0001
            var embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001

            // Generate embeddings
            var embeddings = await embeddingGenerator.GenerateEmbeddingsAsync([text]);

            // Convert ReadOnlyMemory<float> to float[]
            return embeddings.FirstOrDefault().ToArray() ?? Array.Empty<float>();
        }

        // Get chat completion
        public async static Task<string> GetChatCompletionAsync(HttpClient client, string prompt)
        {
            string url = "http://localhost:11434/api/chat";
            var requestData = new
            {
                model = "phi4-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                stream = false
            };
            string jsonRequest = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return chatResponse?.Message?.Content ?? string.Empty;
        }
    }
}
