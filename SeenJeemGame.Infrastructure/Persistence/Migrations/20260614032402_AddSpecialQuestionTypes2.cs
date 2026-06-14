using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeenJeemGame.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialQuestionTypes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionType",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ClueAdjustedPoints",
                table: "GameTurns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevealedCluesCount",
                table: "GameTurns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ClueAdjustedPoints",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "RevealedCluesCount",
                table: "GameTurns");
        }
    }
}
