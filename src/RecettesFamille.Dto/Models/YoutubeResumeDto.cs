namespace RecettesFamille.Dto.Models;

public class YoutubeResumeDto
{
    public int Id { get; set; }
    public required string Resume { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Travel relationship
    public int? TravelId { get; set; }
    public string? TravelName { get; set; }
}
