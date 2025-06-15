using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <summary>
    /// Migration to add PostgreSQL triggers and functions for automatically setting
    /// <c>CreatedDate</c> and <c>UpdatedDate</c> columns on the <c>Recipes</c> table.
    /// </summary>
    public partial class AddRecipeTimestampsTriggers : Migration
    {
        /// <summary>
        /// Applies the migration by creating the trigger function and trigger
        /// for setting timestamp columns on insert and update.
        /// </summary>
        /// <param name="migrationBuilder">The builder used to construct operations for the migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    CREATE OR REPLACE FUNCTION set_recipe_timestamps()
    RETURNS TRIGGER AS $$
    BEGIN
      IF TG_OP = 'INSERT' THEN
        NEW.""CreatedDate"" := NOW();
        NEW.""UpdatedDate"" := NOW();
      ELSIF TG_OP = 'UPDATE' THEN
        NEW.""UpdatedDate"" := NOW();
      END IF;

      RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;
    ");

            migrationBuilder.Sql(@"
    CREATE TRIGGER trg_set_recipe_timestamps
    BEFORE INSERT OR UPDATE ON ""Recipes""
    FOR EACH ROW
    EXECUTE FUNCTION set_recipe_timestamps();
    ");
        }

        /// <summary>
        /// Reverts the migration by dropping the trigger and trigger function
        /// for the <c>Recipes</c> table.
        /// </summary>
        /// <param name="migrationBuilder">The builder used to construct operations for the migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_set_recipe_timestamps ON ""Recipes"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS set_recipe_timestamps;");
        }
    }
}
