using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Domain.Entities;

public class GameTurn
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameSessionId { get; set; }

    public Guid GameQuestionId { get; set; }

    public Guid MainTeamId { get; set; }

    public Guid SecondTeamId { get; set; }

    public TurnStatus Status { get; set; } = TurnStatus.NotStarted;

    public bool IsDoublePointsUsed { get; set; } = false;

    public bool IsTwoAnswersUsed { get; set; } = false;

    public bool IsStopPlayerUsed { get; set; } = false;

    public Guid? StoppedPlayerId { get; set; }

    public int BasePoints { get; set; }

    public int FinalPoints { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public GameSession GameSession { get; set; } = null!;

    public GameQuestion GameQuestion { get; set; } = null!;

    public Team MainTeam { get; set; } = null!;

    public Team SecondTeam { get; set; } = null!;

    public ICollection<AnswerAttempt> AnswerAttempts { get; set; } = new List<AnswerAttempt>();

    public bool IsTrapUsed { get; set; }

    public Guid? TrapTargetTeamId { get; set; }
}