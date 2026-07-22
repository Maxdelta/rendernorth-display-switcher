using System.Text.Json;
using RenderNorth.DisplaySwitcher.Models;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class LegacyDisplayProfileCompatibilityTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    [Theory]
    [InlineData("game.json", "Legacy Game Display")]
    [InlineData("script.json", "Legacy Script Display")]
    public void LegacyProfileFixture_DeserializesWithoutChangingFormat(string fileName, string friendlyName)
    {
        var profile = JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(Fixture(fileName)), Json);

        Assert.NotNull(profile);
        Assert.Equal(1, profile.FormatVersion);
        Assert.Equal("TEST-MACHINE", profile.MachineName);
        Assert.Empty(profile.Paths);
        Assert.Empty(profile.Modes);
        Assert.Single(profile.Targets);
        Assert.Equal(friendlyName, profile.Targets[0].FriendlyName);
    }

    [Fact]
    public void LegacyProfile_RoundTripPreservesDisplayPayload()
    {
        var original = JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(Fixture("game.json")), Json)!;

        var roundTrip = JsonSerializer.Deserialize<DisplayProfile>(JsonSerializer.Serialize(original, Json), Json)!;

        Assert.Equal(original.FormatVersion, roundTrip.FormatVersion);
        Assert.Equal(original.SavedAt, roundTrip.SavedAt);
        Assert.Equal(original.MachineName, roundTrip.MachineName);
        Assert.Equal(original.Targets[0].AdapterId, roundTrip.Targets[0].AdapterId);
        Assert.Equal(original.Targets[0].TargetId, roundTrip.Targets[0].TargetId);
        Assert.Equal(original.Targets[0].MonitorDevicePath, roundTrip.Targets[0].MonitorDevicePath);
        Assert.Equal(original.Targets[0].FriendlyName, roundTrip.Targets[0].FriendlyName);
    }

    [Fact]
    public void CorruptLegacyProfile_IsRejectedByJsonParser()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(Fixture("corrupt.json")), Json));
    }

    private static string Fixture(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "LegacyProfiles", fileName);
}
