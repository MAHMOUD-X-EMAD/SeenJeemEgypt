using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.QuestionMetadata
{
    public class ThreeCluesMetadata
    {
        public List<string> Clues { get; set; } = new();

        public List<int> PointsByClue { get; set; } = new();
    }
}
