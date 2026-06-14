using SeenJeemGame.Application.Games.Dtos;

namespace SeenJeemGame.Application.Games;

public interface IGamePlayService
{
    Task<SelectedQuestionResponse> SelectQuestionAsync(
        Guid gameSessionId,
        SelectQuestionRequest request);

    Task<RevealAnswerResponse> RevealAnswerAsync(
        Guid gameSessionId,
        Guid gameTurnId);

    Task<AwardPointsResponse> AwardPointsAsync(
        Guid gameSessionId,
        Guid gameTurnId,
        AwardPointsRequest request);

    Task<UseHelpOptionResponse> UseHelpOptionAsync(
    Guid gameSessionId,
    Guid gameTurnId,
    UseHelpOptionRequest request);

    Task<AdjustTeamScoreResponse> AdjustTeamScoreAsync(
    Guid gameSessionId,
    AdjustTeamScoreRequest request);

    Task<RevealNextClueResponse> RevealNextClueAsync(
    Guid gameSessionId,
    Guid gameTurnId);
}