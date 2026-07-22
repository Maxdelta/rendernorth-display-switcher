using RenderNorth.DisplaySwitcher.Domain.Environments;

namespace RenderNorth.DisplaySwitcher.Services;

internal enum CommandKind { ShowGui, ActivateById, ActivateByName, ListEnvironments, LegacyGame, LegacyScript, Invalid }

internal sealed record ParsedCommand(CommandKind Kind, Guid? EnvironmentId = null, string? EnvironmentName = null, bool Silent = false, string? Error = null);
internal sealed record CommandExecutionResult(int ExitCode, string Message, bool ShowGui = false);

internal sealed class CommandLineService
{
    public const int SuccessExitCode = 0;
    public const int ActivationFailedExitCode = 1;
    public const int InvalidArgumentsExitCode = 2;
    public const int MissingEnvironmentExitCode = 4;
    public const int AmbiguousEnvironmentExitCode = 5;
    private readonly EnvironmentManager _manager;
    private readonly AppLogger? _log;
    private readonly TextWriter _output;

    public CommandLineService(EnvironmentManager manager, AppLogger? log = null, TextWriter? output = null)
    {
        _manager = manager; _log = log; _output = output ?? Console.Out;
    }

    public ParsedCommand Parse(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0) return new ParsedCommand(CommandKind.ShowGui);
        var silent = arguments.Any(argument => string.Equals(argument, "--silent", StringComparison.OrdinalIgnoreCase));
        var args = arguments.Where(argument => !string.Equals(argument, "--silent", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (args.Length == 1)
        {
            return args[0].ToLowerInvariant() switch
            {
                "--show" => new ParsedCommand(CommandKind.ShowGui, Silent: silent),
                "--list-environments" => new ParsedCommand(CommandKind.ListEnvironments, Silent: silent),
                "--game" => new ParsedCommand(CommandKind.LegacyGame, Silent: true),
                "--script" => new ParsedCommand(CommandKind.LegacyScript, Silent: true),
                _ => Invalid(arguments)
            };
        }
        if (args.Length == 2)
        {
            var option = args[0].ToLowerInvariant();
            if (option is "--environment-id" or "--activate-environment")
                return Guid.TryParse(args[1], out var id)
                    ? new ParsedCommand(CommandKind.ActivateById, EnvironmentId: id, Silent: true)
                    : new ParsedCommand(CommandKind.Invalid, Silent: silent, Error: "Environment ID must be a valid GUID.");
            if (option == "--environment" && !string.IsNullOrWhiteSpace(args[1]))
                return new ParsedCommand(CommandKind.ActivateByName, EnvironmentName: args[1].Trim(), Silent: true);
        }
        return Invalid(arguments);
    }

    public async Task<CommandExecutionResult> ExecuteAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        var command = Parse(arguments);
        if (command.Kind == CommandKind.Invalid) return Log(new CommandExecutionResult(InvalidArgumentsExitCode, command.Error ?? "Invalid arguments."));
        if (command.Kind == CommandKind.ShowGui) return new CommandExecutionResult(SuccessExitCode, "Show GUI.", true);
        if (command.Kind == CommandKind.ListEnvironments)
        {
            foreach (var environment in _manager.Load().Environments.OrderBy(item => item.SortOrder).ThenBy(item => item.Name))
                await _output.WriteLineAsync($"{environment.Id:D}\t{environment.Name}");
            return new CommandExecutionResult(SuccessExitCode, "Environment list written.");
        }

        var resolution = Resolve(command);
        if (resolution.ExitCode != SuccessExitCode) return Log(new CommandExecutionResult(resolution.ExitCode, resolution.Message));
        var activation = await _manager.ActivateAsync(resolution.EnvironmentId!.Value, cancellationToken);
        return Log(new CommandExecutionResult(activation.Success ? SuccessExitCode : ActivationFailedExitCode, activation.Message));
    }

    private Resolution Resolve(ParsedCommand command)
    {
        var collection = _manager.Load();
        if (command.Kind == CommandKind.ActivateById)
            return collection.Environments.Any(item => item.Id == command.EnvironmentId)
                ? Resolution.Found(command.EnvironmentId!.Value)
                : Resolution.Missing("Environment ID was not found.");
        if (command.Kind == CommandKind.ActivateByName)
        {
            return ResolveByName(collection.Environments, command.EnvironmentName!);
        }
        var alias = command.Kind == CommandKind.LegacyGame ? "game" : "script";
        var aliases = collection.Environments.Where(item => item.LegacyAliases.Any(value => string.Equals(value, alias, StringComparison.OrdinalIgnoreCase))).ToArray();
        if (aliases.Length == 0) return Resolution.Missing($"The legacy {alias} environment no longer exists.");
        if (aliases.Length > 1) return new Resolution(AmbiguousEnvironmentExitCode, $"The legacy alias '{alias}' is ambiguous.", null);
        return Resolution.Found(aliases[0].Id);
    }

    internal static Resolution ResolveByName(IEnumerable<EnvironmentDefinition> environments, string name)
    {
        var matches = environments.Where(item => string.Equals(item.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length == 0) return Resolution.Missing($"Environment '{name}' was not found.");
        if (matches.Length > 1) return new Resolution(AmbiguousEnvironmentExitCode, $"Environment name '{name}' is ambiguous.", null);
        return Resolution.Found(matches[0].Id);
    }

    private CommandExecutionResult Log(CommandExecutionResult result)
    {
        if (result.ExitCode == 0) _log?.Info($"Command completed: {result.Message}"); else _log?.Error($"Command failed with exit code {result.ExitCode}: {result.Message}");
        return result;
    }
    private static ParsedCommand Invalid(IReadOnlyList<string> arguments) => new(CommandKind.Invalid, Error: $"Invalid arguments: {string.Join(' ', arguments)}");
    internal sealed record Resolution(int ExitCode, string Message, Guid? EnvironmentId)
    {
        public static Resolution Found(Guid id) => new(SuccessExitCode, "Found.", id);
        public static Resolution Missing(string message) => new(MissingEnvironmentExitCode, message, null);
    }
}
