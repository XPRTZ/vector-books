using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
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

        public async static Task<string> GetChatCompletionAsync(string prompt)
        {
#pragma warning disable SKEXP0070
            var kernel = Kernel.CreateBuilder()
                .AddOllamaChatCompletion(
                    endpoint: new Uri("http://localhost:11434"),
                    modelId: "phi4-mini")
                .Build();
#pragma warning restore SKEXP0070

#pragma warning disable SKEXP0001
            var chatService = kernel.GetRequiredService<IChatCompletionService>();
#pragma warning restore SKEXP0001

            // Create chat history
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);  // Adds the user message

            // Generate response
            var result = await chatService.GetChatMessageContentAsync(chatHistory);

            return result?.Content ?? string.Empty;
        }

    }
}
