using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlasikhaPlavanieWebsite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Competitions",
                newName: "RulesFilePath");

            migrationBuilder.AddColumn<string>(
                name: "ImageFilePath",
                table: "Competitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegulationFilePath",
                table: "Competitions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFilePath",
                table: "Competitions");

            migrationBuilder.DropColumn(
                name: "RegulationFilePath",
                table: "Competitions");

            migrationBuilder.RenameColumn(
                name: "RulesFilePath",
                table: "Competitions",
                newName: "ImagePath");
        }
    }
}
