using SeenJeemGame.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Domain.Entities
{
    public class FinalRound
    {
        public Guid Id { get; set; }

        public Guid GameSessionId { get; set; }

        public int FinalRoundQuestionId { get; set; }

        public FinalRoundStatus Status { get; set; }
            = FinalRoundStatus.WaitingForWagers;

        public int TimerSeconds { get; set; } = 60;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? WagersLockedAt { get; set; }

        public DateTime? QuestionRevealedAt { get; set; }

        public DateTime? AnswerRevealedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public GameSession GameSession { get; set; } = null!;

        public FinalRoundQuestion Question { get; set; } = null!;

        public ICollection<FinalRoundTeamResult> TeamResults { get; set; }
            = new List<FinalRoundTeamResult>();
    }
}
