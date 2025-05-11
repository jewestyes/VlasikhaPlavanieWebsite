using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlasikhaPlavanieWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddFileMappingExternalLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExternalLink",
                table: "FileMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExternalLink",
                table: "FileMappings");
        }
    }
}
