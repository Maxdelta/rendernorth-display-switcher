namespace RenderNorth.DisplaySwitcher.Services;

internal enum ProfileKind { Game, Script }

internal readonly record struct OperationResult(bool Success, string Message)
{
    public static OperationResult Ok(string message) => new(true, message);
    public static OperationResult Fail(string message) => new(false, message);
}

internal sealed record ApplicationStatus(
    string CurrentProfile,
    string LastSwitchResult,
    DateTimeOffset? LastSuccessfulSwitchAt);
