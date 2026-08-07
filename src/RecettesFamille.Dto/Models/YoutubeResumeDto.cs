namespace RecettesFamille.Dto.Models;

public class YoutubeResumeDto
{
    public int Id { get; set; }
    public required string Resume { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // V2 Features
    /// <summary>
    /// Indique si le résumé est marqué comme favori.
    /// </summary>
    public bool IsFavorite { get; set; } = false;

    /// <summary>
    /// Statut de traitement du résumé.
    /// </summary>
    public SummaryStatus Status { get; set; } = SummaryStatus.ToReview;

    // Travel relationship
    public int? TravelId { get; set; }
    public string? TravelName { get; set; }

    // Categories relationship
    /// <summary>
    /// Liste des identifiants des catégories associées.
    /// </summary>
    public List<int> CategoryIds { get; set; } = [];

    /// <summary>
    /// Liste des noms des catégories associées (pour l'affichage).
    /// </summary>
    public List<string> CategoryNames { get; set; } = [];

    /// <summary>
    /// Liste des couleurs des catégories associées (pour l'affichage).
    /// </summary>
    public List<string?> CategoryColors { get; set; } = [];
}
