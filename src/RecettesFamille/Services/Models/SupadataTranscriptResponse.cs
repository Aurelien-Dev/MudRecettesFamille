namespace RecettesFamille.Services.Models;

/// <summary>
/// Représente la réponse de l'API Supadata pour la récupération d'un transcript YouTube.
/// </summary>
public class SupadataTranscriptResponse
{
    /// <summary>
    /// Le contenu du transcript de la vidéo YouTube.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Le titre de la vidéo YouTube (optionnel selon l'API).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// L'identifiant de la vidéo YouTube.
    /// </summary>
    public string? VideoId { get; set; }

    /// <summary>
    /// La durée de la vidéo en secondes (optionnel).
    /// </summary>
    public int? Duration { get; set; }
}
