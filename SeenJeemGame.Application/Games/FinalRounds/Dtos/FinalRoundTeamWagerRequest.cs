using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.FinalRounds.Dtos;

public class FinalRoundTeamWagerRequest
{
    public Guid TeamId { get; set; }

    public int Wager { get; set; }
}