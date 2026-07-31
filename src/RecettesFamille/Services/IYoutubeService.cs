using RecettesFamille.Dto.Models;

namespace RecettesFamille.Services;

/// <summary>
/// Interface pour la gestion des opérations YouTube incluant l'extraction de transcripts
/// et la génération de résumés via l'intelligence artificielle.
/// </summary>
public interface IYoutubeService
{
    /// <summary>
    /// Génère un résumé complet d'une vidéo YouTube à partir de son URL.
    /// Cette méthode orchestre l'extraction du transcript via l'API Supadata,
    /// la génération du résumé via l'AI, et la sauvegarde en base de données.
    /// </summary>
    /// <param name="youtubeUrl">L'URL complète de la vidéo YouTube (supporte les formats youtube.com/watch?v=, youtu.be/, youtube.com/embed/).</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le résumé de la vidéo YouTube avec toutes les métadonnées.</returns>
    /// <exception cref="ArgumentException">Lancée si l'URL n'est pas une URL YouTube valide.</exception>
    /// <exception cref="HttpRequestException">Lancée si l'appel à l'API Supadata échoue.</exception>
    Task<YoutubeResumeDto> GenerateSummaryFromUrl(string youtubeUrl, CancellationToken cancellationToken = default);
}
