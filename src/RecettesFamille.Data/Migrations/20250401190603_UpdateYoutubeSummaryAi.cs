using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateYoutubeSummaryAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Transcript",
                table: "YoutubeSummarys",
                newName: "Resume");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Resume",
                table: "YoutubeSummarys",
                newName: "Transcript");
        }
    }
}
