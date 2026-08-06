namespace RecettesFamille.Data.EntityModel
{
    public class YoutubeResumeEntity
    {
        public required int Id { get; set; }
        public required string Resume { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Travel relationship
        public int? TravelId { get; set; }
        public TravelEntity? Travel { get; set; }
    }
}
