﻿namespace RecettesFamille.Data.EntityModel
{
    public class YoutubeSummaryRequestEntity
    {
        public required int Id { get; set; }
        public required string Transcript { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
    }
}
