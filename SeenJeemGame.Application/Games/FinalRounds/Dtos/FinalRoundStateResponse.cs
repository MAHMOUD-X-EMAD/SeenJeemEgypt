using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.FinalRounds.Dtos;

public class FinalRoundStateResponse
{
    public Guid FinalRoundId { get; set; }

    public Guid GameSessionId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int TimerSeconds { get; set; }

    public string? QuestionText { get; set; }

    public string? CorrectAnswer { get; set; }

    public string? ImageUrl { get; set; }

    public string? AudioUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? WagersLockedAt { get; set; }

    public DateTime? QuestionRevealedAt { get; set; }

    public DateTime? AnswerRevealedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid? WinnerTeamId { get; set; }

    public string? WinnerTeamName { get; set; }

    public bool IsDraw { get; set; }

    public List<FinalRoundTeamStateDto> Teams { get; set; } = new();
}
