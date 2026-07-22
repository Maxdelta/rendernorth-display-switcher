namespace RenderNorth.DisplaySwitcher.Domain.Modules;

internal sealed record ModuleContext(IServiceProvider? Services = null);
internal sealed record ModuleActivationContext(Guid EnvironmentId, string EnvironmentName, IServiceProvider? Services = null);
internal sealed record ModuleDetectionContext(IServiceProvider? Services = null);

internal sealed record ModuleValidationResult(bool Success, string Message)
{
    public static ModuleValidationResult Valid(string message = "Valid") => new(true, message);
    public static ModuleValidationResult Invalid(string message) => new(false, message);
}

internal sealed record ModuleReadinessResult(bool CanActivate, string Message)
{
    public static ModuleReadinessResult Ready(string message = "Ready") => new(true, message);
    public static ModuleReadinessResult NotReady(string message) => new(false, message);
}

internal sealed record ModuleActivationResult(bool Success, string Message, bool ChangedState = false)
{
    public static ModuleActivationResult Succeeded(string message, bool changedState = false) => new(true, message, changedState);
    public static ModuleActivationResult Failed(string message, bool changedState = false) => new(false, message, changedState);
}

internal sealed record ModuleDetectionResult(bool Matches, string Message)
{
    public static ModuleDetectionResult Match(string message = "Matched") => new(true, message);
    public static ModuleDetectionResult NoMatch(string message = "Did not match") => new(false, message);
}

internal sealed record ModuleDeactivationResult(bool Success, string Message)
{
    public static ModuleDeactivationResult Succeeded(string message = "Deactivated") => new(true, message);
    public static ModuleDeactivationResult Failed(string message) => new(false, message);
}
