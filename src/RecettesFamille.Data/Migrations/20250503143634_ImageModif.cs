using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImageModif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "BlockImageEntity",
                type: "BYTEA",
                maxLength: 1048576,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BYTEA",
                oldMaxLength: 1048576);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "BlockImageEntity",
                type: "BYTEA",
                maxLength: 1048576,
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BYTEA",
                oldMaxLength: 1048576,
                oldNullable: true);
        }
    }
}
