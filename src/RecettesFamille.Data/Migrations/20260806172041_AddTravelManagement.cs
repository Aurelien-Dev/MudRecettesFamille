using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TravelId",
                table: "YoutubeSummarys",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Travels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Travels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YoutubeSummarys_TravelId",
                table: "YoutubeSummarys",
                column: "TravelId");

            migrationBuilder.AddForeignKey(
                name: "FK_YoutubeSummarys_Travels_TravelId",
                table: "YoutubeSummarys",
                column: "TravelId",
                principalTable: "Travels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YoutubeSummarys_Travels_TravelId",
                table: "YoutubeSummarys");

            migrationBuilder.DropTable(
                name: "Travels");

            migrationBuilder.DropIndex(
                name: "IX_YoutubeSummarys_TravelId",
                table: "YoutubeSummarys");

            migrationBuilder.DropColumn(
                name: "TravelId",
                table: "YoutubeSummarys");
        }
    }
}
