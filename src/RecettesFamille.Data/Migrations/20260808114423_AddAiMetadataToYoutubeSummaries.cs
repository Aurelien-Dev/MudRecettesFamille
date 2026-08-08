using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecettesFamille.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiMetadataToYoutubeSummaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AiMetadataAnalyzedAt",
                table: "YoutubeSummarys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiMetadataError",
                table: "YoutubeSummarys",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiMetadataStatus",
                table: "YoutubeSummarys",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MainCountryConfidence",
                table: "YoutubeSummarys",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainCountryIsoCode",
                table: "YoutubeSummarys",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainCountryName",
                table: "YoutubeSummarys",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiMetadataAnalyzedAt",
                table: "YoutubeSummarys");

            migrationBuilder.DropColumn(
                name: "AiMetadataError",
                table: "YoutubeSummarys");

            migrationBuilder.DropColumn(
                name: "AiMetadataStatus",
                table: "YoutubeSummarys");

            migrationBuilder.DropColumn(
                name: "MainCountryConfidence",
                table: "YoutubeSummarys");

            migrationBuilder.DropColumn(
                name: "MainCountryIsoCode",
                table: "YoutubeSummarys");

            migrationBuilder.DropColumn(
                name: "MainCountryName",
                table: "YoutubeSummarys");
        }
    }
}
