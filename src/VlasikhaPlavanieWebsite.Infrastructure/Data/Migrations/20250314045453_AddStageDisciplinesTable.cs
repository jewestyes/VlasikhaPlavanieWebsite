using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlasikhaPlavanieWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddStageDisciplinesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StageDisciplines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistancesJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageDisciplines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageDisciplines_RegistrationStage_StageId",
                        column: x => x.StageId,
                        principalTable: "RegistrationStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StageDisciplines_StageId",
                table: "StageDisciplines",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StageDisciplines");
        }
    }
}
