using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quranTranslationExtractor.Migrations
{
    /// <inheritdoc />
    public partial class ChangedAndAddNewProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TafsirIdInSura",
                table: "Tafsirs",
                newName: "TafsirIndexInSura");

            migrationBuilder.RenameColumn(
                name: "SuraId",
                table: "Tafsirs",
                newName: "SuraIndex");

            migrationBuilder.RenameColumn(
                name: "AyatId",
                table: "Tafsirs",
                newName: "AyatIndex");

            migrationBuilder.RenameColumn(
                name: "SuraId",
                table: "Ayats",
                newName: "SuraIndex");

            migrationBuilder.AlterColumn<int>(
                name: "TotalVerses",
                table: "Suras",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TafsirIndexInSura",
                table: "Tafsirs",
                newName: "TafsirIdInSura");

            migrationBuilder.RenameColumn(
                name: "SuraIndex",
                table: "Tafsirs",
                newName: "SuraId");

            migrationBuilder.RenameColumn(
                name: "AyatIndex",
                table: "Tafsirs",
                newName: "AyatId");

            migrationBuilder.RenameColumn(
                name: "SuraIndex",
                table: "Ayats",
                newName: "SuraId");

            migrationBuilder.AlterColumn<string>(
                name: "TotalVerses",
                table: "Suras",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
