using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeenJeemGame.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalRoundFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinalRoundQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalRoundQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinalRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinalRoundQuestionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TimerSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WagersLockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuestionRevealedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnswerRevealedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalRounds_FinalRoundQuestions_FinalRoundQuestionId",
                        column: x => x.FinalRoundQuestionId,
                        principalTable: "FinalRoundQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinalRounds_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalRoundTeamResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinalRoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Wager = table.Column<int>(type: "int", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    ScoreDelta = table.Column<int>(type: "int", nullable: false),
                    WagerLockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnswerSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalRoundTeamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalRoundTeamResults_FinalRounds_FinalRoundId",
                        column: x => x.FinalRoundId,
                        principalTable: "FinalRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinalRoundTeamResults_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalRoundQuestions_IsActive",
                table: "FinalRoundQuestions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FinalRounds_FinalRoundQuestionId",
                table: "FinalRounds",
                column: "FinalRoundQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalRounds_GameSessionId",
                table: "FinalRounds",
                column: "GameSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalRoundTeamResults_FinalRoundId_TeamId",
                table: "FinalRoundTeamResults",
                columns: new[] { "FinalRoundId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalRoundTeamResults_TeamId",
                table: "FinalRoundTeamResults",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinalRoundTeamResults");

            migrationBuilder.DropTable(
                name: "FinalRounds");

            migrationBuilder.DropTable(
                name: "FinalRoundQuestions");
        }
    }
}
