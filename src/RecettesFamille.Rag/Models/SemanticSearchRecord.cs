using Microsoft.Extensions.VectorData;

namespace RecettesFamille.Rag.Models;

/// <summary>
/// Vector store record representing a semantic search chunk for an entity.
/// Each entity generates multiple specialized chunks (Metadata, Description, Ingredients, Instructions).
/// </summary>
public class SemanticSearchRecord
{
    /// <summary>Unique key for this chunk (e.g., "recipe_5_chunk_0").</summary>
    [VectorStoreKey]
    public string Key { get; set; } = string.Empty;

    /// <summary>ID of the source entity (e.g., recipe ID).</summary>
    [VectorStoreData(IsIndexed = true)]
    public int RecipeId { get; set; }

    /// <summary>Type of chunk: Metadata, Description, Ingredients, Instructions.</summary>
    [VectorStoreData(IsIndexed = true)]
    public string ChunkType { get; set; } = string.Empty;

    /// <summary>Category of the entity (e.g., "Plat principal").</summary>
    [VectorStoreData(IsIndexed = true)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Searchable text content of this chunk.</summary>
    [VectorStoreData]
    public string Text { get; set; } = string.Empty;

    /// <summary>Embedding vector for semantic similarity search.</summary>
    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
