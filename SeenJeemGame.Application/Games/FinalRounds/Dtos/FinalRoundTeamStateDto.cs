using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.FinalRounds.Dtos;

public class FinalRoundTeamStateDto
{
    public Guid TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public int ScoreBeforeFinalRound { get; set; }

    public int CurrentScore { get; set; }

    public int? Wager { get; set; }

    public bool IsWagerLocked { get; set; }

    public string? AnswerText { get; set; }

    public bool? IsCorrect { get; set; }

    public int ScoreDelta { get; set; }
}
