using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDatesOnRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedDate",
                table: "Recipes",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "UpdatedDate",
                table: "Recipes",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Recipes");
        }
    }
}
