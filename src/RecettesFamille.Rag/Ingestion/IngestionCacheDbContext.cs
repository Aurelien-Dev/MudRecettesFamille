using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RecettesFamille.Rag.Ingestion;

/// <summary>
/// A DbContext that keeps track of which documents have been ingested.
/// This makes it possible to avoid re-ingesting documents that have not changed,
/// and to delete documents that have been removed from the underlying source.
/// </summary>
public class IngestionCacheDbContext : DbContext
{
    public IngestionCacheDbContext(DbContextOptions<IngestionCacheDbContext> options) : base(options)
    {
    }

    public DbSet<IngestedDocument> Documents { get; set; } = default!;
    public DbSet<IngestedRecord> Records { get; set; } = default!;

    public static void Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<IngestionCacheDbContext>();

        // Apply any pending migrations
        db.Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names with RAG prefix
        modelBuilder.Entity<IngestedDocument>().ToTable("RagIngestionDocuments");
        modelBuilder.Entity<IngestedRecord>().ToTable("RagIngestionRecords");

        // Configure relationships
        modelBuilder.Entity<IngestedDocument>().HasMany(d => d.Records).WithOne().HasForeignKey(r => r.DocumentId).OnDelete(DeleteBehavior.Cascade);
    }
}
