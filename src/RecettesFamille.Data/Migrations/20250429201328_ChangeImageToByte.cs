using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeImageToByte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Supprimer la colonne existante
            migrationBuilder.DropColumn(
                name: "Image",
                table: "BlockImageEntity");

            // Recréer la colonne en BYTEA
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "BlockImageEntity",
                type: "BYTEA",
                maxLength: 1048576,
                nullable: true);

            // Ajouter la contrainte de taille
            migrationBuilder.AddCheckConstraint(
                name: "CHK_Photo_ImageDataSize",
                table: "BlockImageEntity",
                sql: "octet_length(\"Image\") <= 1048576");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer la contrainte
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Photo_ImageDataSize",
                table: "BlockImageEntity");

            // Supprimer la colonne BYTEA
            migrationBuilder.DropColumn(
                name: "Image",
                table: "BlockImageEntity");

            // Recréer l'ancienne colonne en texte
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "BlockImageEntity",
                type: "text",
                nullable: false);
        }
    }
}