using System;
using System.Collections.Generic;
using System.Text;
using SeenJeemGame.Application.Games.FinalRounds.Dtos;

namespace SeenJeemGame.Application.Games.FinalRounds;

public interface IFinalRoundService
{
    Task<FinalRoundStateResponse> StartFinalRoundAsync(
        Guid gameSessionId);

    Task<FinalRoundStateResponse?> GetFinalRoundAsync(
        Guid gameSessionId);

    Task<FinalRoundStateResponse> LockWagersAsync(
        Guid gameSessionId,
        Guid finalRoundId,
        LockFinalRoundWagersRequest request);

    Task<FinalRoundStateResponse> RevealQuestionAsync(
        Guid gameSessionId,
        Guid finalRoundId);

    Task<FinalRoundStateResponse> RevealAnswerAsync(
        Guid gameSessionId,
        Guid finalRoundId);

    Task<FinalRoundStateResponse> CompleteFinalRoundAsync(
        Guid gameSessionId,
        Guid finalRoundId,
        CompleteFinalRoundRequest request);
}
