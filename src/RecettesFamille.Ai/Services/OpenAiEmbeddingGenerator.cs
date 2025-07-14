using Microsoft.Extensions.AI;

namespace RecettesFamille.Ai.Services;

// Minimal IEmbeddingGenerator abstraction for compatibility
public interface IEmbeddingGenerator
{
    Task<ReadOnlyMemory<float>> GenerateVectorAsync(string text);
}

public class OpenAiEmbeddingGenerator(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator) : IEmbeddingGenerator
{
    public async Task<ReadOnlyMemory<float>> GenerateVectorAsync(string text)
    {
        var result = await embeddingGenerator.GenerateAsync(text);

        // Assuming Embedding<float> has a property or method to get the vector, e.g., .Vector or .Values
        // Replace 'Values' with the actual property if different
        return result.Vector;
    }
}