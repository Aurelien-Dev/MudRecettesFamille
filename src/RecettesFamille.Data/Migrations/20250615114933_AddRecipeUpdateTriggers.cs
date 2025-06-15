using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <summary>
    /// Migration to add PostgreSQL triggers and functions for automatically updating
    /// the <c>UpdatedDate</c> column of the <c>Recipes</c> table when related entities
    /// (<c>BlockInstructionEntity</c>, <c>BlockImageEntity</c>, <c>BlockIngredientListEntity</c>)
    /// are inserted, updated, or deleted.
    /// </summary>
    public partial class AddRecipeUpdateTriggers : Migration
    {
        /// <summary>
        /// Applies the migration by creating the trigger function and triggers
        /// for updating the <c>UpdatedDate</c> column of the <c>Recipes</c> table
        /// when related block entities are modified.
        /// </summary>
        /// <param name="migrationBuilder">The builder used to construct operations for the migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the PL/pgSQL function
            migrationBuilder.Sql(@"
    CREATE OR REPLACE FUNCTION update_recipe_updateddate()
    RETURNS TRIGGER AS $$
    DECLARE
      recipe_id INTEGER;
    BEGIN
      IF TG_OP = 'DELETE' THEN
        recipe_id := OLD.""RecipeId"";
      ELSE
        recipe_id := NEW.""RecipeId"";
      END IF;

      UPDATE ""Recipes""
      SET ""UpdatedDate"" = NOW()
      WHERE ""Id"" = recipe_id;

      RETURN NULL;
    END;
    $$ LANGUAGE plpgsql;
    ");

            // Triggers for BlockInstructionEntity
            migrationBuilder.Sql(@"
    CREATE TRIGGER trg_recipe_update_instruction_insert
    AFTER INSERT ON ""BlockInstructionEntity""
    FOR EACH ROW
    EXECUTE FUNCTION update_recipe_updateddate();

    CREATE TRIGGER trg_recipe_update_instruction_update
    AFTER UPDATE ON ""BlockInstructionEntity""
    FOR EACH ROW
    WHEN (OLD IS DISTINCT FROM NEW)
    EXECUTE FUNCTION update_recipe_updateddate();

    CREATE TRIGGER trg_recipe_update_instruction_delete
    AFTER DELETE ON ""BlockInstructionEntity""
    FOR EACH ROW
    EXECUTE FUNCTION update_recipe_updateddate();
    ");

            // Triggers for BlockImageEntity
            migrationBuilder.Sql(@"
    CREATE TRIGGER trg_recipe_update_image_insert
    AFTER INSERT ON ""BlockImageEntity""
    FOR EACH ROW
    EXECUTE FUNCTION update_recipe_updateddate();

    CREATE TRIGGER trg_recipe_update_image_update
    AFTER UPDATE ON ""BlockImageEntity""
    FOR EACH ROW
    WHEN (OLD IS DISTINCT FROM NEW)
    EXECUTE FUNCTION update_recipe_updateddate();

    CREATE TRIGGER trg_recipe_update_image_delete
    AFTER DELETE ON ""BlockImageEntity""
    FOR EACH ROW
    EXECUTE FUNCTION update_recipe_updateddate();
    ");

            // Triggers for BlockIngredientListEntity
            migrationBuilder.Sql(@"
    CREATE TRIGGER trg_recipe_update_ingredient_insert
    AFTER INSERT ON ""BlockIngredientListEntity""
    FOR EACH ROW
    EXECUTE FUNCTION update_recipe_updateddate();

    CREATE TRIGGER trg_recipe_update_ingredient_update
    AFTER UPDATE ON ""BlockIngredientListEntity""
    FOR EACH ROW
    WHEN (OLD IS DISTINCT FROM NEW)
    EXECUTE FUNCTION update_recipe_updateddate();

    CREATE TRIGGER trg_recipe_update_ingredient_delete
    AFTER DELETE ON ""BlockIngredientListEntity""
    FOR EACH ROW
    EXECUTE FUNCTION update_recipe_updateddate();
    ");
        }

        /// <summary>
        /// Reverts the migration by dropping the triggers and trigger function
        /// for updating the <c>UpdatedDate</c> column of the <c>Recipes</c> table.
        /// </summary>
        /// <param name="migrationBuilder">The builder used to construct operations for the migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_instruction_insert ON ""BlockInstructionEntity"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_instruction_update ON ""BlockInstructionEntity"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_instruction_delete ON ""BlockInstructionEntity"";");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_image_insert ON ""BlockImageEntity"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_image_update ON ""BlockImageEntity"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_image_delete ON ""BlockImageEntity"";");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_ingredient_insert ON ""BlockIngredientListEntity"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_ingredient_update ON ""BlockIngredientListEntity"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipe_update_ingredient_delete ON ""BlockIngredientListEntity"";");

            // Drop the function
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS update_recipe_updateddate;");
        }
    }
}
