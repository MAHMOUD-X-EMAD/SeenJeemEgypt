using Microsoft.EntityFrameworkCore;
using SeenJeemGame.Domain.Entities;
using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();

    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GameCategory> GameCategories => Set<GameCategory>();
    public DbSet<GameQuestion> GameQuestions => Set<GameQuestion>();

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<TeamHelpOption> TeamHelpOptions => Set<TeamHelpOption>();

    public DbSet<GameTurn> GameTurns => Set<GameTurn>();
    public DbSet<AnswerAttempt> AnswerAttempts => Set<AnswerAttempt>();
    public DbSet<ScoreTransaction> ScoreTransactions => Set<ScoreTransaction>();

    public DbSet<FinalRoundQuestion> FinalRoundQuestions =>
    Set<FinalRoundQuestion>();

    public DbSet<FinalRound> FinalRounds =>
        Set<FinalRound>();

    public DbSet<FinalRoundTeamResult> FinalRoundTeamResults =>
        Set<FinalRoundTeamResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCategories(modelBuilder);
        ConfigureQuestions(modelBuilder);

        ConfigureGameSessions(modelBuilder);
        ConfigureGameCategories(modelBuilder);
        ConfigureGameQuestions(modelBuilder);

        ConfigureTeams(modelBuilder);
        ConfigurePlayers(modelBuilder);
        ConfigureTeamHelpOptions(modelBuilder);

        ConfigureGameTurns(modelBuilder);
        ConfigureAnswerAttempts(modelBuilder);
        ConfigureScoreTransactions(modelBuilder);

        SeedCategories(modelBuilder);

        modelBuilder.Entity<FinalRoundQuestion>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CategoryName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(x => x.CorrectAnswer)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(1000);

            entity.Property(x => x.AudioUrl)
                .HasMaxLength(1000);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<FinalRound>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.TimerSeconds)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasIndex(x => x.GameSessionId)
                .IsUnique();

            entity.HasOne(x => x.GameSession)
                .WithOne(x => x.FinalRound)
                .HasForeignKey<FinalRound>(x => x.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Question)
                .WithMany(x => x.FinalRounds)
                .HasForeignKey(x => x.FinalRoundQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinalRoundTeamResult>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Wager)
                .IsRequired();

            entity.Property(x => x.AnswerText)
                .HasMaxLength(1000);

            entity.Property(x => x.ScoreDelta)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.FinalRoundId,
                x.TeamId
            }).IsUnique();

            entity.HasOne(x => x.FinalRound)
                .WithMany(x => x.TeamResults)
                .HasForeignKey(x => x.FinalRoundId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Team)
                .WithMany(x => x.FinalRoundResults)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });
    }

    private static void ConfigureQuestions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(x => x.CorrectAnswer)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(x => x.Difficulty)
                .HasConversion<int>();

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(1000);

            entity.Property(x => x.AudioUrl)
                .HasMaxLength(1000);

            entity.Property(x => x.VideoUrl)
                .HasMaxLength(1000);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.QuestionType)
            .IsRequired()
            .HasDefaultValue(QuestionType.Standard);

            entity.Property(x => x.MetadataJson)
                .HasColumnType("nvarchar(max)");

            entity.HasIndex(x => new { x.CategoryId, x.Difficulty });
        });
    }

    private static void ConfigureGameSessions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoomCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.HasIndex(x => x.RoomCode)
                .IsUnique();
        });
    }

    private static void ConfigureGameCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameCategory>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.GameSession)
                .WithMany(x => x.GameCategories)
                .HasForeignKey(x => x.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.GameSessionId, x.CategoryId })
                .IsUnique();

            entity.HasIndex(x => new { x.GameSessionId, x.Order })
                .IsUnique();
        });
    }

    private static void ConfigureGameQuestions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameQuestion>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Difficulty)
                .HasConversion<int>();

            entity.HasOne(x => x.GameSession)
                .WithMany(x => x.GameQuestions)
                .HasForeignKey(x => x.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.GameSessionId, x.QuestionId })
                .IsUnique();

            entity.HasIndex(x => new { x.GameSessionId, x.CategoryId, x.Difficulty });
        });
    }

    private static void ConfigureTeams(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasOne(x => x.GameSession)
                .WithMany(x => x.Teams)
                .HasForeignKey(x => x.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.GameSessionId, x.TurnOrder })
                .IsUnique();

            entity.HasIndex(x => new { x.GameSessionId, x.Name })
                .IsUnique();
        });
    }

    private static void ConfigurePlayers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasOne(x => x.Team)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTeamHelpOptions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamHelpOption>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .HasConversion<int>();

            entity.HasOne(x => x.Team)
                .WithMany(x => x.HelpOptions)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.TeamId, x.Type })
                .IsUnique();
        });
    }

    private static void ConfigureGameTurns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameTurn>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.HasOne(x => x.GameSession)
                .WithMany(x => x.Turns)
                .HasForeignKey(x => x.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.GameQuestion)
                .WithMany()
                .HasForeignKey(x => x.GameQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MainTeam)
                .WithMany()
                .HasForeignKey(x => x.MainTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SecondTeam)
                .WithMany()
                .HasForeignKey(x => x.SecondTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.GameQuestionId)
                .IsUnique();

            entity.Property(x => x.BlindRankingRevealOrderJson)
                .HasColumnType("nvarchar(max)");
        });
    }

    private static void ConfigureAnswerAttempts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerAttempt>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AnswerText)
                .HasMaxLength(1000);

            entity.HasOne(x => x.GameTurn)
                .WithMany(x => x.AnswerAttempts)
                .HasForeignKey(x => x.GameTurnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.GameTurnId, x.TeamId });
        });
    }

    private static void ConfigureScoreTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScoreTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasOne(x => x.GameSession)
                .WithMany()
                .HasForeignKey(x => x.GameSessionId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Team)
                .WithMany(x => x.ScoreTransactions)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.GameTurn)
                .WithMany()
                .HasForeignKey(x => x.GameTurnId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(x => new { x.GameSessionId, x.TeamId });
        });
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "دين", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 2, Name = "جغرافيا", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 3, Name = "تاريخ", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 4, Name = "كورة", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 5, Name = "رياضيات", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 6, Name = "معلومات عامة", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 7, Name = "أفلام ومسلسلات", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 8, Name = "علوم", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 9, Name = "تكنولوجيا", IsActive = true, CreatedAt = createdAt },
            new Category { Id = 10, Name = "ألغاز", IsActive = true, CreatedAt = createdAt }
        );
    }
}