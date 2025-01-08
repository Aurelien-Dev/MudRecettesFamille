using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;

namespace RecettesFamille.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<RecetteEntity> Recettes { get; set; }

        public DbSet<BlockInstruction> BlockInstructions { get; set; }
        public DbSet<BlockImage> BlockImages { get; set; }
        public DbSet<BlockIngredientList> BlockIngredientLists { get; set; }
        public DbSet<IngredientDto> IngredientDtos { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RecetteEntity>().HasKey(c => c.Id);
            builder.Entity<RecetteEntity>().Property(b => b.Id).ValueGeneratedOnAdd();

            builder.Entity<BlockBase>().UseTpcMappingStrategy();

            builder.Entity<BlockBase>().HasKey(c => c.Id);
            builder.Entity<BlockBase>().Property(b => b.Id).ValueGeneratedOnAdd();

            builder.Entity<IngredientDto>().HasKey(c => c.Id);
            builder.Entity<IngredientDto>().Property(b => b.Id).ValueGeneratedOnAdd();

            base.OnModelCreating(builder);
        }

    }
}
