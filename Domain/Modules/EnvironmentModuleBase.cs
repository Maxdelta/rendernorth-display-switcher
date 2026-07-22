namespace RenderNorth.DisplaySwitcher.Domain.Modules;

internal abstract class EnvironmentModuleBase : IEnvironmentModule
{
    public abstract string ModuleType { get; }
    public abstract int SchemaVersion { get; }
    public abstract int ActivationOrder { get; }
    public abstract void Load(ModuleDocument document);
    public abstract ModuleDocument Save();
    public virtual Task InitializeAsync(ModuleContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    public virtual Task<ModuleValidationResult> ValidateAsync(ModuleContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleValidationResult.Valid());
    public virtual Task<ModuleReadinessResult> CanActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleReadinessResult.Ready());
    public virtual Task<ModuleActivationResult> PreActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleActivationResult.Succeeded("Pre-activation complete."));
    public abstract Task<ModuleActivationResult> ActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken);
    public virtual Task<ModuleActivationResult> PostActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleActivationResult.Succeeded("Post-activation complete."));
    public abstract Task<ModuleDetectionResult> DetectAsync(ModuleDetectionContext context, CancellationToken cancellationToken);
    public virtual Task<ModuleDeactivationResult> DeactivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleDeactivationResult.Succeeded("No deactivation action was required."));
}
