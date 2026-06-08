namespace SeenJeemGame.Application.Games.Dtos;

public class GameSetupResponse
{
    public Guid GameSessionId { get; set; }

    public string RoomCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public List<GameTeamDto> Teams { get; set; } = new();

    public List<GameBoardCategoryDto> Categories { get; set; } = new();
}