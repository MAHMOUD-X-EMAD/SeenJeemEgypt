using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.FinalRounds.Dtos;

public class LockFinalRoundWagersRequest
{
    public List<FinalRoundTeamWagerRequest> Teams { get; set; } = new();
}
