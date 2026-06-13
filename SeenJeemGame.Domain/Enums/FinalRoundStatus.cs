using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Domain.Enums
{
    public enum FinalRoundStatus
    {
        WaitingForWagers = 1,
        WagersLocked = 2,
        QuestionRevealed = 3,
        AnswerRevealed = 4,
        Completed = 5
    }
}
