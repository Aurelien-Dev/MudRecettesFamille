using RecettesFamille.Services.Models;

namespace RecettesFamille.ServicesExternal;

/// <summary>
/// Interface pour les appels à l'API Supadata.
/// </summary>
public interface ISupadataService
{
    /// <summary>
    /// Récupère le transcript d'une vidéo YouTube via l'API Supadata.
    /// </summary>
    /// <param name="videoId">L'identifiant de la vidéo YouTube.</param>
    /// <param name="cancellationToken">Token d'annulation pour l'opération asynchrone.</param>
    /// <returns>La réponse de l'API Supadata contenant le transcript et les métadonnées.</returns>
    Task<SupadataTranscriptResponse> GetYoutubeTranscriptAsync(string videoId, CancellationToken cancellationToken = default);
}
