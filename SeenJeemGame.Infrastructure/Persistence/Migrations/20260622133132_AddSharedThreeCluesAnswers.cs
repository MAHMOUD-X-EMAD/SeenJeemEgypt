using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeenJeemGame.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedThreeCluesAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainTeamLockedClueNumber",
                table: "GameTurns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MainTeamLockedPoints",
                table: "GameTurns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondTeamLockedClueNumber",
                table: "GameTurns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondTeamLockedPoints",
                table: "GameTurns",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainTeamLockedClueNumber",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "MainTeamLockedPoints",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "SecondTeamLockedClueNumber",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "SecondTeamLockedPoints",
                table: "GameTurns");
        }
    }
}
