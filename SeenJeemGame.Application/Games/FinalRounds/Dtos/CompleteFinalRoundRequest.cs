using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.FinalRounds.Dtos;

public class CompleteFinalRoundRequest
{
    public List<FinalRoundTeamAnswerRequest> Teams { get; set; } = new();
}
