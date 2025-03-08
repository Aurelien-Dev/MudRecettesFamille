using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using System.Diagnostics;

namespace RecettesFamille.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<RecipeEntity> Recipes { get; set; }
        public DbSet<AiConsumptionEntity> AiConsumptions { get; set; }
        public DbSet<PromptEntity> Prompts { get; set; }
        public DbSet<TagEntity> Tags { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RecipeEntity>().HasKey(c => c.Id);
            builder.Entity<RecipeEntity>().Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Entity<BlockBaseEntity>().UseTpcMappingStrategy();

            builder.Entity<BlockBaseEntity>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.HasOne(c => c.Recipe)
                      .WithMany(c => c.BlocksInstructions)
                      .HasForeignKey(e => e.RecipeId)
                      .IsRequired();
            });

            builder.Entity<BlockImageEntity>();
            builder.Entity<BlockIngredientListEntity>();
            builder.Entity<BlockInstructionEntity>();

            builder.Entity<IngredientEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.IngredientList)
                      .WithMany(e => e.Ingredients)
                      .HasForeignKey(e => e.IngredientListId);
            });

            builder.Entity<PromptEntity>().HasKey(c => c.Id);
            builder.Entity<BlockBaseEntity>().Property(c => c.Id).ValueGeneratedOnAdd();

            base.OnModelCreating(builder);
        }

        public static (bool, string, string) TriggerBackup()
        {
            try
            {
                var fileName = $"backup_{DateTime.UtcNow:s}.backup";
                var command = $"PGPASSWORD=PGUserPwd pg_dump -h recettesfamille.data -p 5432 -U pguser -d recettesfamilledb -F c -f wwwroot/backups/{fileName}";

                if (!Directory.Exists("wwwroot/backups"))
                    Directory.CreateDirectory("wwwroot/backups");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process is null) throw new InvalidOperationException("Process not exist.");

                var output = process.StandardOutput.ReadToEnd();
                Console.WriteLine($"Output : {output}");

                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"Backup terminé avec succès : {fileName}");
                    return (true, "Backup terminé avec succès : {fileName}", fileName);
                }
                else
                    throw new InvalidOperationException($"Erreur lors du backup : {error}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors du backup : {ex.Message}");
                return (false, $"Exception lors du backup : {ex.Message}", string.Empty);
            }
        }
    }
}
