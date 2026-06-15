using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeenJeemGame.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlindRankingSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlindRankingRevealOrderJson",
                table: "GameTurns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MainTeamCorrectPositions",
                table: "GameTurns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevealedRankingItemsCount",
                table: "GameTurns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SecondTeamCorrectPositions",
                table: "GameTurns",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlindRankingRevealOrderJson",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "MainTeamCorrectPositions",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "RevealedRankingItemsCount",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "SecondTeamCorrectPositions",
                table: "GameTurns");
        }
    }
}
