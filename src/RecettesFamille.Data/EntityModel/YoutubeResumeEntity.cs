namespace RecettesFamille.Data.EntityModel
{
    public class YoutubeResumeEntity
    {
        public required int Id { get; set; }
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
        /// Statut de traitement du résumé (À consulter, Consulté, Retenu, Écarté).
        /// </summary>
        public SummaryStatus Status { get; set; } = SummaryStatus.ToReview;

        // Travel relationship
        public int? TravelId { get; set; }
        public TravelEntity? Travel { get; set; }

        // Categories relationship (many-to-many)
        /// <summary>
        /// Catégories associées à ce résumé YouTube.
        /// </summary>
        public ICollection<CategoryEntity> Categories { get; set; } = [];
    }
}
