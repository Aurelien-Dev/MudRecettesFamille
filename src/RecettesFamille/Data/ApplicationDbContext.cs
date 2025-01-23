using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Extensions;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.RecipeSubEntity;
using System.Diagnostics;

namespace RecettesFamille.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<RecipeEntity> Recettes { get; set; }
        public DbSet<AiConsumptionEntity> AiConsumptions { get; set; }
        public DbSet<PromptEntity> Prompts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RecipeEntity>().HasKey(c => c.Id);
            builder.Entity<RecipeEntity>().Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Entity<BlockBase>().UseTpcMappingStrategy();

            builder.Entity<BlockBase>().HasKey(c => c.Id);
            builder.Entity<BlockBase>().Property(c => c.Id).ValueGeneratedOnAdd();
            builder.Entity<BlockBase>().HasOne(c => c.Recipe).WithMany(c => c.BlocksInstructions).IsRequired();

            builder.Entity<BlockImageEntity>();
            builder.Entity<BlockIngredientListEntity>();
            builder.Entity<BlockInstructionEntity>();

            builder.Entity<IngredientEntity>().HasKey(c => c.Id);
            builder.Entity<IngredientEntity>().Property(c => c.Id).ValueGeneratedOnAdd();
            builder.Entity<IngredientEntity>().HasOne(c => c.IngredientList).WithMany(c => c.Ingredients).HasForeignKey(c => c.IngredientListId);

            builder.Entity<PromptEntity>().HasKey(c => c.Id);
            builder.Entity<BlockBase>().Property(c => c.Id).ValueGeneratedOnAdd();

            base.OnModelCreating(builder);
        }

        public (bool, string, string) TriggerBackup()
        {
            try
            {
                string fileName = $"backup_{DateTime.UtcNow.ToIsoDateString()}-{DateTime.UtcNow.ToShortTimeString()}.backup";   
                string command = $"PGPASSWORD=PGUserPwd pg_dump -h recettes.atelier-cremazie.com -p 5442 -U pguser -d recettesfamilledb -F c -f wwwroot/backups/{fileName}";

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

                using (var process = Process.Start(processInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"Backup terminé avec succès : {fileName}");
                        return (true, "Backup terminé avec succès : {fileName}", fileName);
                    }
                    else
                        throw new Exception($"Erreur lors du backup : {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors du backup : {ex.Message}");
                return (false, $"Exception lors du backup : {ex.Message}", string.Empty);
            }
        }
    }
}
