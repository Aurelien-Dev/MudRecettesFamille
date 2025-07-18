﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RecettesFamille.Ai.Services.Ingestion;

// A DbContext that keeps track of which documents have been ingested.
// This makes it possible to avoid re-ingesting documents that have not changed,
// and to delete documents that have been removed from the underlying source.
public class IngestionCacheDbContext(DbContextOptions<IngestionCacheDbContext> options) : DbContext(options)
{
    public DbSet<IngestedDocument> Documents { get; set; } = default!;
    public DbSet<IngestedRecord> Records { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<IngestedDocument>().HasMany(d => d.Records).WithOne().HasForeignKey(r => r.DocumentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class IngestedDocument
{
    // TODO: Make Id+SourceId a composite key
    public required int Id { get; set; }
    public required string SourceId { get; set; }
    public required string Version { get; set; }
    public List<IngestedRecord> Records { get; set; } = [];
}

public class IngestedRecord
{
    public required string Id { get; set; }
    public required int DocumentId { get; set; }
}
