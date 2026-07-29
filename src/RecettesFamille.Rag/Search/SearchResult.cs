using RecettesFamille.Rag.Models;

namespace RecettesFamille.Rag.Search;

/// <summary>
/// Result of a semantic search including the record and its similarity score.
/// </summary>
/// <param name="Record">The semantic search record found.</param>
/// <param name="Score">Similarity score (0.0 to 1.0, higher is more similar).</param>
public record SearchResult(SemanticSearchRecord Record, double Score);
