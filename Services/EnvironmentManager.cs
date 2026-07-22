using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Domain.Modules;

namespace RenderNorth.DisplaySwitcher.Services;

internal sealed record EnvironmentOperationResult(bool Success, string Message, EnvironmentDefinition? Environment = null)
{
    public static EnvironmentOperationResult Ok(string message, EnvironmentDefinition? environment = null) => new(true, message, environment);
    public static EnvironmentOperationResult Fail(string message) => new(false, message);
}

internal sealed record EnvironmentActivationResult(
    bool Success,
    string Message,
    Guid? EnvironmentId,
    IReadOnlyList<string> ModuleResults,
    IReadOnlyList<string> UnsupportedModules);

internal sealed record EnvironmentDetectionResult(EnvironmentDefinition? Environment, IReadOnlyList<string> UnsupportedModules);

internal sealed class EnvironmentManager
{
    private readonly EnvironmentRepository _repository;
    private readonly EnvironmentModuleRegistry _registry;
    private readonly AppLogger? _log;
    private readonly Func<DateTimeOffset> _clock;
    private readonly Func<Guid> _newId;

    public EnvironmentManager(EnvironmentRepository repository, EnvironmentModuleRegistry registry, AppLogger? log = null,
        Func<DateTimeOffset>? clock = null, Func<Guid>? newId = null)
    {
        _repository = repository;
        _registry = registry;
        _log = log;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _newId = newId ?? Guid.NewGuid;
    }

    public EnvironmentCollection Load() => _repository.Load();

    public EnvironmentOperationResult Create(string name, string? description = null, string icon = "workspace", string? category = null,
        IReadOnlyList<ModuleDocument>? modules = null)
    {
        var collection = _repository.Load();
        if (!ValidUniqueName(collection, name, null, out var error)) return EnvironmentOperationResult.Fail(error);
        var now = _clock();
        var environment = new EnvironmentDefinition
        {
            Id = _newId(), Name = name.Trim(), Description = NullIfWhiteSpace(description), Icon = icon,
            Category = NullIfWhiteSpace(category), CreatedAt = now, UpdatedAt = now,
            SortOrder = collection.Environments.Count == 0 ? 0 : collection.Environments.Max(item => item.SortOrder) + 1,
            Modules = modules?.Select(module => module.Clone()).ToList() ?? []
        };
        collection.Environments.Add(environment); _repository.Save(collection);
        return EnvironmentOperationResult.Ok($"Created environment '{environment.Name}'.", environment);
    }

    public EnvironmentOperationResult Rename(Guid id, string name)
    {
        var collection = _repository.Load(); var environment = collection.Environments.FirstOrDefault(item => item.Id == id);
        if (environment is null) return EnvironmentOperationResult.Fail("Environment not found.");
        if (!ValidUniqueName(collection, name, id, out var error)) return EnvironmentOperationResult.Fail(error);
        environment.Name = name.Trim(); environment.UpdatedAt = _clock(); _repository.Save(collection);
        return EnvironmentOperationResult.Ok($"Renamed environment to '{environment.Name}'.", environment);
    }

    public EnvironmentOperationResult Duplicate(Guid id)
    {
        var collection = _repository.Load(); var source = collection.Environments.FirstOrDefault(item => item.Id == id);
        if (source is null) return EnvironmentOperationResult.Fail("Environment not found.");
        var name = UniqueCopyName(collection, source.Name); var now = _clock();
        var duplicate = new EnvironmentDefinition
        {
            Id = _newId(), Name = name, Description = source.Description, Icon = source.Icon, Category = source.Category,
            Accent = source.Accent, Tags = [.. source.Tags], CreatedAt = now, UpdatedAt = now, IsFavorite = source.IsFavorite,
            SortOrder = collection.Environments.Count == 0 ? 0 : collection.Environments.Max(item => item.SortOrder) + 1,
            LegacyAliases = [], Modules = source.Modules.Select(module => module.Clone()).ToList(),
            Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.OrdinalIgnoreCase)
        };
        collection.Environments.Add(duplicate); _repository.Save(collection);
        return EnvironmentOperationResult.Ok($"Duplicated '{source.Name}' as '{name}'.", duplicate);
    }

    public EnvironmentOperationResult Delete(Guid id)
    {
        var collection = _repository.Load(); var environment = collection.Environments.FirstOrDefault(item => item.Id == id);
        if (environment is null) return EnvironmentOperationResult.Fail("Environment not found.");
        collection.Environments.Remove(environment);
        if (collection.ActiveEnvironmentId == id) collection.ActiveEnvironmentId = null;
        NormalizeSortOrder(collection); _repository.Save(collection);
        return EnvironmentOperationResult.Ok($"Deleted environment '{environment.Name}'.");
    }

    public EnvironmentOperationResult Move(Guid id, int offset)
    {
        var collection = _repository.Load(); var ordered = collection.Environments.OrderBy(item => item.SortOrder).ThenBy(item => item.Name).ToList();
        var index = ordered.FindIndex(item => item.Id == id); if (index < 0) return EnvironmentOperationResult.Fail("Environment not found.");
        var destination = Math.Clamp(index + offset, 0, ordered.Count - 1); if (destination == index) return EnvironmentOperationResult.Ok("Environment order is unchanged.", ordered[index]);
        var item = ordered[index]; ordered.RemoveAt(index); ordered.Insert(destination, item);
        collection.Environments = ordered; NormalizeSortOrder(collection); item.UpdatedAt = _clock(); _repository.Save(collection);
        return EnvironmentOperationResult.Ok($"Moved environment '{item.Name}'.", item);
    }

    public EnvironmentOperationResult Update(EnvironmentDefinition updated)
    {
        var collection = _repository.Load(); var index = collection.Environments.FindIndex(item => item.Id == updated.Id);
        if (index < 0) return EnvironmentOperationResult.Fail("Environment not found.");
        if (!ValidUniqueName(collection, updated.Name, updated.Id, out var error)) return EnvironmentOperationResult.Fail(error);
        updated.Name = updated.Name.Trim(); updated.UpdatedAt = _clock(); collection.Environments[index] = updated; _repository.Save(collection);
        return EnvironmentOperationResult.Ok($"Saved environment '{updated.Name}'.", updated);
    }

    public async Task<EnvironmentActivationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = _repository.Load(); var environment = collection.Environments.FirstOrDefault(item => item.Id == id);
        if (environment is null) return Failure("Environment not found.", null, [], []);
        var results = new List<string>(); var unsupported = new List<string>(); var modules = Materialize(environment, unsupported).OrderBy(module => module.ActivationOrder).ToList();
        if (modules.Count == 0) return SaveFailure(collection, environment, "Environment has no supported enabled capabilities.", results, unsupported);
        var context = new ModuleActivationContext(environment.Id, environment.Name);
        var initialized = new ModuleContext();
        var rollback = new List<IEnvironmentModule>();
        try
        {
            foreach (var module in modules) { cancellationToken.ThrowIfCancellationRequested(); await module.InitializeAsync(initialized, cancellationToken); results.Add($"{module.ModuleType}: initialized"); }
            foreach (var module in modules)
            {
                var validation = await module.ValidateAsync(initialized, cancellationToken); results.Add($"{module.ModuleType}: {validation.Message}");
                if (!validation.Success) return SaveFailure(collection, environment, $"{module.ModuleType} validation failed: {validation.Message}", results, unsupported);
            }
            foreach (var module in modules)
            {
                var readiness = await module.CanActivateAsync(context, cancellationToken); results.Add($"{module.ModuleType}: {readiness.Message}");
                if (!readiness.CanActivate) return SaveFailure(collection, environment, $"{module.ModuleType} is not ready: {readiness.Message}", results, unsupported);
            }
            foreach (var module in modules)
            {
                var result = await module.PreActivateAsync(context, cancellationToken); results.Add($"{module.ModuleType}: {result.Message}");
                if (!result.Success) return await RollbackFailure(collection, environment, $"{module.ModuleType} pre-activation failed: {result.Message}", results, unsupported, rollback, context, cancellationToken);
                rollback.Add(module);
            }
            foreach (var module in modules)
            {
                var result = await module.ActivateAsync(context, cancellationToken); results.Add($"{module.ModuleType}: {result.Message}");
                if (!result.Success) return await RollbackFailure(collection, environment, $"{module.ModuleType} activation failed: {result.Message}", results, unsupported, rollback, context, cancellationToken);
            }
            foreach (var module in modules)
            {
                var result = await module.PostActivateAsync(context, cancellationToken); results.Add($"{module.ModuleType}: {result.Message}");
                if (!result.Success) return await RollbackFailure(collection, environment, $"{module.ModuleType} post-activation failed: {result.Message}", results, unsupported, rollback, context, cancellationToken);
            }
            foreach (var module in modules)
            {
                var detection = await module.DetectAsync(new ModuleDetectionContext(), cancellationToken); results.Add($"{module.ModuleType}: {detection.Message}");
                if (!detection.Matches) return await RollbackFailure(collection, environment, $"{module.ModuleType} verification failed: {detection.Message}", results, unsupported, rollback, context, cancellationToken);
            }
            var message = $"{environment.Name} activated successfully."; collection.ActiveEnvironmentId = environment.Id;
            collection.ActivationStatus.LastResult = message; collection.ActivationStatus.LastSuccessfulAt = _clock(); _repository.Save(collection);
            Info($"Environment activation succeeded: {environment.Name}. {string.Join(" | ", results)}");
            return new EnvironmentActivationResult(true, message, environment.Id, results, unsupported);
        }
        catch (OperationCanceledException) { return await RollbackFailure(collection, environment, "Environment activation was canceled.", results, unsupported, rollback, context, CancellationToken.None); }
        catch (Exception exception) { Error($"Unhandled activation failure for {environment.Name}", exception); return await RollbackFailure(collection, environment, exception.Message, results, unsupported, rollback, context, CancellationToken.None); }
    }

    public async Task<EnvironmentDetectionResult> DetectAsync(CancellationToken cancellationToken = default)
    {
        var collection = _repository.Load(); var unsupported = new List<string>();
        foreach (var environment in collection.Environments.OrderBy(item => item.SortOrder).ThenBy(item => item.Name))
        {
            var modules = Materialize(environment, unsupported).OrderBy(module => module.ActivationOrder).ToList(); if (modules.Count == 0) continue;
            var matches = true;
            foreach (var module in modules) if (!(await module.DetectAsync(new ModuleDetectionContext(), cancellationToken)).Matches) { matches = false; break; }
            if (matches) return new EnvironmentDetectionResult(environment, unsupported);
        }
        return new EnvironmentDetectionResult(null, unsupported);
    }

    private IEnumerable<IEnvironmentModule> Materialize(EnvironmentDefinition environment, List<string> unsupported)
    {
        foreach (var document in environment.Modules.Where(module => module.IsEnabled))
        {
            if (_registry.TryCreate(document, out var module)) yield return module!;
            else { var message = $"{environment.Name}: unsupported capability '{document.Type}' schema {document.SchemaVersion}"; unsupported.Add(message); Info(message); }
        }
    }

    private async Task<EnvironmentActivationResult> RollbackFailure(EnvironmentCollection collection, EnvironmentDefinition environment, string message,
        List<string> results, List<string> unsupported, List<IEnvironmentModule> rollback, ModuleActivationContext context, CancellationToken cancellationToken)
    {
        foreach (var module in rollback.AsEnumerable().Reverse())
        {
            try { var result = await module.DeactivateAsync(context, cancellationToken); results.Add($"{module.ModuleType} rollback: {result.Message}"); }
            catch (Exception exception) { results.Add($"{module.ModuleType} rollback failed: {exception.Message}"); Error($"{module.ModuleType} rollback failed", exception); }
        }
        return SaveFailure(collection, environment, message, results, unsupported);
    }

    private EnvironmentActivationResult SaveFailure(EnvironmentCollection collection, EnvironmentDefinition environment, string message, List<string> results, List<string> unsupported)
    {
        collection.ActiveEnvironmentId = null; collection.ActivationStatus.LastResult = message; _repository.Save(collection);
        Error($"Environment activation failed: {environment.Name}. {message}"); return Failure(message, environment.Id, results, unsupported);
    }
    private static EnvironmentActivationResult Failure(string message, Guid? id, IReadOnlyList<string> results, IReadOnlyList<string> unsupported) => new(false, message, id, results, unsupported);
    private static bool ValidUniqueName(EnvironmentCollection collection, string name, Guid? excluding, out string error)
    {
        if (string.IsNullOrWhiteSpace(name)) { error = "Environment name is required."; return false; }
        if (collection.Environments.Any(item => item.Id != excluding && string.Equals(item.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))) { error = $"An environment named '{name.Trim()}' already exists."; return false; }
        error = ""; return true;
    }
    private static string UniqueCopyName(EnvironmentCollection collection, string source)
    {
        for (var number = 1; ; number++) { var candidate = number == 1 ? $"{source} Copy" : $"{source} Copy {number}"; if (collection.Environments.All(item => !string.Equals(item.Name, candidate, StringComparison.OrdinalIgnoreCase))) return candidate; }
    }
    private static void NormalizeSortOrder(EnvironmentCollection collection) { var index = 0; foreach (var item in collection.Environments.OrderBy(item => item.SortOrder).ThenBy(item => item.Name)) item.SortOrder = index++; }
    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private void Info(string message) => _log?.Info(message);
    private void Error(string message, Exception? exception = null) => _log?.Error(message, exception);
}
