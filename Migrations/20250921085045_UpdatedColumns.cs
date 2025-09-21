using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quranTranslationExtractor.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SuraIndex",
                table: "Suras",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TotalVerses",
                table: "Suras",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AyatIndex",
                table: "Ayats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuraIndex",
                table: "Suras");

            migrationBuilder.DropColumn(
                name: "TotalVerses",
                table: "Suras");

            migrationBuilder.DropColumn(
                name: "AyatIndex",
                table: "Ayats");
        }
    }
}
