namespace RenderNorth.DisplaySwitcher.Domain.Modules;

internal sealed class EnvironmentModuleRegistry
{
    private readonly Dictionary<string, Func<IEnvironmentModule>> _factories = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string moduleType, Func<IEnvironmentModule> factory)
    {
        if (string.IsNullOrWhiteSpace(moduleType)) throw new ArgumentException("Module type is required.", nameof(moduleType));
        ArgumentNullException.ThrowIfNull(factory);
        if (!_factories.TryAdd(moduleType, factory)) throw new InvalidOperationException($"Module type '{moduleType}' is already registered.");
    }

    public bool IsSupported(string moduleType) => _factories.ContainsKey(moduleType);

    public bool TryCreate(ModuleDocument document, out IEnvironmentModule? module)
    {
        module = null;
        if (!_factories.TryGetValue(document.Type, out var factory)) return false;
        module = factory();
        module.Load(document);
        return true;
    }
}
