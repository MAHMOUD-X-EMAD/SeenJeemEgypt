using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Domain.Entities
{
    public class FinalRoundTeamResult
    {
        public Guid Id { get; set; }

        public Guid FinalRoundId { get; set; }

        public Guid TeamId { get; set; }

        public int Wager { get; set; }

        public string? AnswerText { get; set; }

        public bool? IsCorrect { get; set; }

        public int ScoreDelta { get; set; }

        public DateTime? WagerLockedAt { get; set; }

        public DateTime? AnswerSubmittedAt { get; set; }

        public FinalRound FinalRound { get; set; } = null!;

        public Team Team { get; set; } = null!;
    }
}
