using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quranTranslationExtractor.Migrations
{
    /// <inheritdoc />
    public partial class InitializedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ayats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SuraId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ayats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tafsirs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SuraId = table.Column<int>(type: "INTEGER", nullable: false),
                    AyatId = table.Column<int>(type: "INTEGER", nullable: false),
                    TafsirIdInSura = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tafsirs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ayats");

            migrationBuilder.DropTable(
                name: "Suras");

            migrationBuilder.DropTable(
                name: "Tafsirs");
        }
    }
}
