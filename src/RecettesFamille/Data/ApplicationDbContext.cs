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
        public DbSet<GptConsumptionEntity> GptConsumptions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RecipeEntity>().HasKey(c => c.Id);
            builder.Entity<RecipeEntity>().Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Entity<BlockBase>().UseTpcMappingStrategy();

            builder.Entity<BlockBase>().HasKey(c => c.Id);
            builder.Entity<BlockBase>().Property(c => c.Id).ValueGeneratedOnAdd();
            builder.Entity<BlockBase>().HasOne(c => c.Recipe).WithMany(c => c.BlocksInstructions).IsRequired();

            builder.Entity<BlockImageEntity>().Property(c => c.HalfPage).HasDefaultValue(true);
            builder.Entity<BlockIngredientListEntity>().Property(c => c.HalfPage).HasDefaultValue(true);
            builder.Entity<BlockInstructionEntity>().Property(c => c.HalfPage).HasDefaultValue(false);


            builder.Entity<IngredientEntity>().HasKey(c => c.Id);
            builder.Entity<IngredientEntity>().Property(c => c.Id).ValueGeneratedOnAdd();
            builder.Entity<IngredientEntity>().HasOne(c => c.IngredientList).WithMany(c => c.Ingredients).HasForeignKey(c => c.IngredientListId);

            base.OnModelCreating(builder);
        }

        public (bool, string) TriggerBackup()
        {
            try
            {
                string fileName = $"backup_{DateTime.UtcNow.ToIsoDateString()}-{DateTime.UtcNow.ToShortTimeString()}.backup";
                string command = $"PGPASSWORD=PGUserPwd pg_dump -h recettes.atelier-cremazie.com -p 5442 -U pguser -d recettesfamilledb -F c -f wwwroot/{fileName}";

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
                        return (true, "Backup terminé avec succès : {fileName}");
                    }
                    else
                        throw new Exception($"Erreur lors du backup : {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors du backup : {ex.Message}");
                return (false, $"Exception lors du backup : {ex.Message}");
            }
        }
    }
}
