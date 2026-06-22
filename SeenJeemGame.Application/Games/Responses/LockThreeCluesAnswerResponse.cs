using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.Responses;

public sealed class LockThreeCluesAnswerResponse
{
    public Guid GameTurnId { get; set; }

    public Guid TeamId { get; set; }

    public int LockedClueNumber { get; set; }

    public int LockedPoints { get; set; }

    public int? MainTeamLockedClueNumber { get; set; }

    public int? MainTeamLockedPoints { get; set; }

    public int? SecondTeamLockedClueNumber { get; set; }

    public int? SecondTeamLockedPoints { get; set; }

    public bool BothTeamsLocked { get; set; }
}