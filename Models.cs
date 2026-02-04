namespace HowWasYourRankgame;

public class AppConfig
{
    public string RiotApiKey { get; set; } = string.Empty;
    public string DiscordWebhookUrl { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string TagLine { get; set; } = string.Empty;
}

public record AccountDto(string puuid, string gameName, string tagLine);
public record SummonerDto(string id, string accountId, string puuid, string name, long summonerLevel);
