using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.RecipeSubEntity;

namespace RecettesFamille.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<RecipeEntity> Recettes { get; set; }

        public DbSet<BlockInstructionEntity> BlockInstructions { get; set; }
        public DbSet<BlockImageEntity> BlockImages { get; set; }
        public DbSet<BlockIngredientListEntity> BlockIngredientLists { get; set; }
        public DbSet<IngredientEntity> Ingredients { get; set; }

        public DbSet<GptConsumptionEntity> GptConsumptions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RecipeEntity>().HasKey(c => c.Id);
            builder.Entity<RecipeEntity>().Property(b => b.Id).ValueGeneratedOnAdd();

            builder.Entity<BlockBase>().UseTpcMappingStrategy();

            builder.Entity<BlockBase>().HasKey(c => c.Id);
            builder.Entity<BlockBase>().Property(b => b.Id).ValueGeneratedOnAdd();

            builder.Entity<IngredientEntity>().HasKey(c => c.Id);
            builder.Entity<IngredientEntity>().Property(b => b.Id).ValueGeneratedOnAdd();

            base.OnModelCreating(builder);
        }

    }
}
