using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlasikhaPlavanieWebsite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Registrationcompetition_CompetitionId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_competitionDisciplines_Registrationcompetition_CompetitionId",
                table: "competitionDisciplines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_competitionDisciplines",
                table: "competitionDisciplines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Registrationcompetition",
                table: "Registrationcompetition");

            migrationBuilder.RenameTable(
                name: "competitionDisciplines",
                newName: "CompetitionDisciplines");

            migrationBuilder.RenameTable(
                name: "Registrationcompetition",
                newName: "Competitions");

            migrationBuilder.RenameIndex(
                name: "IX_competitionDisciplines_CompetitionId",
                table: "CompetitionDisciplines",
                newName: "IX_CompetitionDisciplines_CompetitionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionDisciplines",
                table: "CompetitionDisciplines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Competitions",
                table: "Competitions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionDisciplines_Competitions_CompetitionId",
                table: "CompetitionDisciplines",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Competitions_CompetitionId",
                table: "Orders",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionDisciplines_Competitions_CompetitionId",
                table: "CompetitionDisciplines");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Competitions_CompetitionId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Competitions",
                table: "Competitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionDisciplines",
                table: "CompetitionDisciplines");

            migrationBuilder.RenameTable(
                name: "Competitions",
                newName: "Registrationcompetition");

            migrationBuilder.RenameTable(
                name: "CompetitionDisciplines",
                newName: "competitionDisciplines");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionDisciplines_CompetitionId",
                table: "competitionDisciplines",
                newName: "IX_competitionDisciplines_CompetitionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Registrationcompetition",
                table: "Registrationcompetition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_competitionDisciplines",
                table: "competitionDisciplines",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Registrationcompetition_CompetitionId",
                table: "Orders",
                column: "CompetitionId",
                principalTable: "Registrationcompetition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_competitionDisciplines_Registrationcompetition_CompetitionId",
                table: "competitionDisciplines",
                column: "CompetitionId",
                principalTable: "Registrationcompetition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
