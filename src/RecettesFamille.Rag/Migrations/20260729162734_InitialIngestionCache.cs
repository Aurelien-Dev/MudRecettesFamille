using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Rag.Migrations
{
    /// <inheritdoc />
    public partial class InitialIngestionCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RagIngestionDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagIngestionDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RagIngestionRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DocumentId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagIngestionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RagIngestionRecords_RagIngestionDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "RagIngestionDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RagIngestionRecords_DocumentId",
                table: "RagIngestionRecords",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RagIngestionRecords");

            migrationBuilder.DropTable(
                name: "RagIngestionDocuments");
        }
    }
}
