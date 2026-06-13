using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.FinalRounds.Dtos;

public class FinalRoundTeamAnswerRequest
{
    public Guid TeamId { get; set; }

    public string? AnswerText { get; set; }

    public bool IsCorrect { get; set; }
}