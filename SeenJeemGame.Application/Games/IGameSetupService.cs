using SeenJeemGame.Application.Games.Dtos;

namespace SeenJeemGame.Application.Games;

public interface IGameSetupService
{
    Task<GameSetupResponse> CreateGameAsync(CreateGameRequest request);

    Task<GameSetupResponse?> GetGameBoardAsync(Guid gameSessionId);
}