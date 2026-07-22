using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Models;

namespace RenderNorth.DisplaySwitcher.Modules.Display;

internal sealed class DisplayModule : EnvironmentModuleBase
{
    public const string Type = "display";
    public const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions Json = new() { IncludeFields = true, PropertyNameCaseInsensitive = true };
    private readonly DisplayModuleService _service;
    private DisplayProfile? _rollback;

    public DisplayModule(DisplayModuleService service) => _service = service;
    public DisplayProfile Configuration { get; private set; } = new();
    public override string ModuleType => Type;
    public override int SchemaVersion => CurrentSchemaVersion;
    public override int ActivationOrder => 100;

    public override void Load(ModuleDocument document)
    {
        if (!string.Equals(document.Type, Type, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException($"Cannot load module type '{document.Type}' as Displays.");
        if (document.SchemaVersion != CurrentSchemaVersion) throw new InvalidDataException($"Unsupported Displays schema {document.SchemaVersion}.");
        Configuration = document.Data.Deserialize<DisplayProfile>(Json) ?? throw new InvalidDataException("The Displays configuration is empty.");
    }

    public override ModuleDocument Save() => new()
    {
        Type = Type,
        SchemaVersion = CurrentSchemaVersion,
        ActivationOrder = ActivationOrder,
        IsEnabled = true,
        Data = JsonSerializer.SerializeToElement(Configuration, Json)
    };

    public void SetConfiguration(DisplayProfile configuration) => Configuration = configuration;

    public override Task<ModuleValidationResult> ValidateAsync(ModuleContext context, CancellationToken cancellationToken)
    {
        try { cancellationToken.ThrowIfCancellationRequested(); _service.Validate(Configuration); return Task.FromResult(ModuleValidationResult.Valid("Displays are valid and connected.")); }
        catch (Exception exception) { return Task.FromResult(ModuleValidationResult.Invalid(exception.Message)); }
    }

    public override Task<ModuleReadinessResult> CanActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken)
    {
        var result = ValidateAsync(new ModuleContext(context.Services), cancellationToken).GetAwaiter().GetResult();
        return Task.FromResult(result.Success ? ModuleReadinessResult.Ready("Displays are ready.") : ModuleReadinessResult.NotReady(result.Message));
    }

    public override Task<ModuleActivationResult> PreActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken)
    {
        try { cancellationToken.ThrowIfCancellationRequested(); _rollback = _service.Capture(); return Task.FromResult(ModuleActivationResult.Succeeded("Captured the previous display configuration.")); }
        catch (Exception exception) { return Task.FromResult(ModuleActivationResult.Failed(exception.Message)); }
    }

    public override Task<ModuleActivationResult> ActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken)
    {
        try { cancellationToken.ThrowIfCancellationRequested(); _service.Apply(Configuration); return Task.FromResult(ModuleActivationResult.Succeeded("Applied the display configuration.", true)); }
        catch (Exception exception) { return Task.FromResult(ModuleActivationResult.Failed(exception.Message)); }
    }

    public override Task<ModuleActivationResult> PostActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_service.Matches(Configuration, _service.Capture())
                ? ModuleActivationResult.Succeeded("Verified the display configuration.")
                : ModuleActivationResult.Failed("Windows accepted the request but the resulting display topology did not match."));
        }
        catch (Exception exception) { return Task.FromResult(ModuleActivationResult.Failed(exception.Message)); }
    }

    public override Task<ModuleDetectionResult> DetectAsync(ModuleDetectionContext context, CancellationToken cancellationToken)
    {
        try { cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult(_service.Matches(Configuration, _service.Capture()) ? ModuleDetectionResult.Match("Displays match.") : ModuleDetectionResult.NoMatch("Displays do not match.")); }
        catch (Exception exception) { return Task.FromResult(ModuleDetectionResult.NoMatch(exception.Message)); }
    }

    public override Task<ModuleDeactivationResult> DeactivateAsync(ModuleActivationContext context, CancellationToken cancellationToken)
    {
        if (_rollback is null) return Task.FromResult(ModuleDeactivationResult.Succeeded("No display rollback was required."));
        try { cancellationToken.ThrowIfCancellationRequested(); _service.Apply(_rollback); return Task.FromResult(ModuleDeactivationResult.Succeeded("Restored the previous display configuration.")); }
        catch (Exception exception) { return Task.FromResult(ModuleDeactivationResult.Failed(exception.Message)); }
    }
}
