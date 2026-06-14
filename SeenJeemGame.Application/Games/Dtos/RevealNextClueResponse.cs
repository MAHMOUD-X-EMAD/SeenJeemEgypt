using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.Dtos
{
    public class RevealNextClueResponse
    {
        public Guid GameTurnId { get; set; }

        public int RevealedCluesCount { get; set; }

        public List<string> RevealedClues { get; set; } = new();

        public int ClueAdjustedPoints { get; set; }

        public int FinalPoints { get; set; }

        public bool HasMoreClues { get; set; }
    }
}
