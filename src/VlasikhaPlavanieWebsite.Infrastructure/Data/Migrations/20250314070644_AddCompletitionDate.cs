using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlasikhaPlavanieWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletitionDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompetitionDate",
                table: "RegistrationStage",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompetitionDate",
                table: "RegistrationStage");
        }
    }
}
