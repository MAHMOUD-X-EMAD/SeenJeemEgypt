using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Domain.Entities
{
    public class FinalRoundQuestion
    {
        public int Id { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string CorrectAnswer { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? AudioUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FinalRound> FinalRounds { get; set; }
            = new List<FinalRound>();
    }
}
