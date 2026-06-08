using SeenJeemGame.Domain.Enums;

namespace SeenJeemGame.Application.Games.Dtos;

public class CreateGameRequest
{
    public List<int> CategoryIds { get; set; } = new();

    public string TeamOneName { get; set; } = string.Empty;

    public string TeamTwoName { get; set; } = string.Empty;

    public List<HelpOptionType> HelpOptions { get; set; } = new();
}