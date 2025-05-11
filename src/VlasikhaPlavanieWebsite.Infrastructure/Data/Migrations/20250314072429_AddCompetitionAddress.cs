using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlasikhaPlavanieWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompetitionAddress",
                table: "RegistrationStage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompetitionAddress",
                table: "RegistrationStage");
        }
    }
}
