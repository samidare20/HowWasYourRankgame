using Xunit;
using HowWasYourRankgame;

namespace HowWasYourRankgame.Tests;

public class ConfigTests
{
    [Fact]
    public void AppConfig_Initialization_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new AppConfig();

        // Assert
        Assert.Equal(string.Empty, config.RiotApiKey);
        Assert.Equal(string.Empty, config.DiscordWebhookUrl);
        Assert.Equal(string.Empty, config.GameName);
        Assert.Equal(string.Empty, config.TagLine);
    }

    [Fact]
    public void AccountDto_ShouldStoreValuesCorrectly()
    {
        // Arrange
        var puuid = "test-puuid";
        var gameName = "TestGame";
        var tagLine = "KR1";

        // Act
        var dto = new AccountDto(puuid, gameName, tagLine);

        // Assert
        Assert.Equal(puuid, dto.puuid);
        Assert.Equal(gameName, dto.gameName);
        Assert.Equal(tagLine, dto.tagLine);
    }
}