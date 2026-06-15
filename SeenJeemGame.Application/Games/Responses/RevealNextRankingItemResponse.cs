using System;
using System.Collections.Generic;
using System.Text;

namespace SeenJeemGame.Application.Games.Responses;

public sealed class RevealNextRankingItemResponse
{
    public Guid GameTurnId { get; set; }

    public int RevealedRankingItemsCount { get; set; }

    public List<string> RevealedRankingItems { get; set; } = new();

    public string CurrentItem { get; set; } = string.Empty;

    public bool HasMoreRankingItems { get; set; }
}
