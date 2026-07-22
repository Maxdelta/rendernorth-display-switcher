namespace RenderNorth.DisplaySwitcher.Domain.Modules;

internal interface IEnvironmentModule
{
    string ModuleType { get; }
    int SchemaVersion { get; }
    int ActivationOrder { get; }
    void Load(ModuleDocument document);
    ModuleDocument Save();
    Task InitializeAsync(ModuleContext context, CancellationToken cancellationToken);
    Task<ModuleValidationResult> ValidateAsync(ModuleContext context, CancellationToken cancellationToken);
    Task<ModuleReadinessResult> CanActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken);
    Task<ModuleActivationResult> PreActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken);
    Task<ModuleActivationResult> ActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken);
    Task<ModuleActivationResult> PostActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken);
    Task<ModuleDetectionResult> DetectAsync(ModuleDetectionContext context, CancellationToken cancellationToken);
    Task<ModuleDeactivationResult> DeactivateAsync(ModuleActivationContext context, CancellationToken cancellationToken);
}
