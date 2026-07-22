# RenderNorth Environments Architecture Vision

## Purpose

RenderNorth Environments is a Windows desktop workspace platform that lets a user select a complete working context for what they are doing. An environment may eventually coordinate displays, audio, applications, window layouts, streaming software, lighting, and other capabilities.

Version 0.4 implements only the Displays capability. Displays are the first module, not the product boundary.

## Environment Domain

An `Environment` is the primary user-facing workspace definition. It owns:

- a permanent GUID `Id`;
- a mutable, case-insensitively unique `Name`;
- optional description and category;
- a controlled built-in icon identifier;
- a symbolic RenderNorth accent identifier;
- optional user-defined tags;
- created and updated timestamps;
- favorite and sort-order state;
- compatibility-only legacy aliases;
- an ordered collection of capability documents;
- narrowly scoped extension metadata that must not replace core domain fields.

Names are presentation. IDs are identity. Renaming an environment must never invalidate commands or shortcuts.

Suggested initial categories are Gaming, Streaming, Development, Work, Creative, Presentation, Travel, and Custom. Categories and tags remain lightweight values in v0.4 rather than separately managed entities.

## Capabilities and Modules

Internally, an environment capability is implemented through `IEnvironmentModule`. The UI uses terms such as Displays, Audio, Applications, Window Layout, Capabilities, and Configuration; users are not required to understand the internal module system.

The v0.4 module registry contains only `DisplayModule`. Empty placeholder modules are prohibited. A future capability must register a real implementation through `EnvironmentModuleRegistry` and participate in the common lifecycle.

The architecture boundary is:

```text
EnvironmentManager
    -> Environment
        -> ordered IEnvironmentModule instances
            -> DisplayModule
                -> DisplayModuleService
                    -> native Windows display engine
```

`EnvironmentManager` must remain module-agnostic. It must not test for `DisplayModule`, switch on module types, call Windows display APIs, or interpret display persistence.

## Module Persistence

`EnvironmentCollection` has its own repository schema version. Each module document has an independent schema version:

```text
ModuleDocument
    Type
    SchemaVersion
    ActivationOrder
    IsEnabled
    Data
```

Known documents are materialized through `EnvironmentModuleRegistry`. Unknown module documents remain intact as raw persisted documents. An older build may report an unknown capability as unsupported, but it must not activate, rewrite, or silently discard its data.

Collection migrations and module migrations evolve independently. A change to a future Audio schema must not require a Display schema rewrite.

## Module Lifecycle

The lifecycle is asynchronous and cancellation-aware at the orchestration boundary, even where a safe native implementation remains synchronous:

1. `InitializeAsync`
2. `ValidateAsync`
3. `CanActivateAsync`
4. `PreActivateAsync`
5. `ActivateAsync`
6. `PostActivateAsync`
7. `DetectAsync`
8. optional `DeactivateAsync`

Modules expose `ModuleType`, `SchemaVersion`, and `ActivationOrder`, plus load/save persistence behavior. Default no-op lifecycle helpers are permitted so a module is not forced to invent work for every hook.

Modules activate sequentially in ascending `ActivationOrder`. Concurrent module activation is prohibited in v0.4.

## Activation Transaction

An activation is successful only when every required supported module succeeds:

1. Resolve the environment by permanent ID, unique name, or approved legacy alias.
2. Materialize supported, enabled modules through the registry.
3. Sort modules by `ActivationOrder`.
4. Initialize and validate all modules.
5. Confirm all modules can activate.
6. Capture module rollback state where required.
7. Run pre-activation, activation, and post-activation sequentially.
8. Verify detected state where applicable.
9. Persist the environment as active only after the complete pipeline succeeds.
10. Log an activation summary containing every module outcome.

On failure, forward activation stops. Modules that changed state are rolled back in reverse activation order. A failure must not falsely mark an environment active. CLI activation returns a documented nonzero exit code and silent activation never displays a popup.

Display rollback retains the existing proven behavior: capture the current display configuration before applying the saved configuration, verify the resulting topology, and restore the captured configuration after failure.

## Display Capability Boundary

`DisplayModule` owns module identity, its display configuration document, lifecycle integration, and translation to generic module results.

`DisplayModuleService` exclusively owns:

- `QueryDisplayConfig` capture;
- machine and connected-target validation;
- `SetDisplayConfig` application;
- primary-display, duplicate, extend, position, resolution, and refresh information;
- expected-versus-active topology comparison;
- rollback capture and restoration;
- connected-display descriptions.

Native display structures must not leak into `EnvironmentManager`, `EnvironmentRepository`, generic module contracts, or UI code. The verified native display implementation is preserved unless a demonstrated defect requires a focused change.

## Repository and Data Safety

Installed data remains under the existing Velopack root `UserData` directory. Portable data remains beside the portable executable. Repository writes must be atomic and validated before replacement.

Valid user data must never be silently discarded. Before migration or schema replacement, the application creates a recoverable backup. Unknown module payloads are round-tripped unchanged. Duplicate environment IDs and case-insensitive duplicate names are rejected.

Legacy `game.json` and `script.json` files migrate to environments named Game Mode and Script Mode with aliases `game` and `script`. The exact display configuration payload is preserved. Migration is logged, backup-first, idempotent, and leaves legacy files untouched.

Users may rename or delete migrated environments. Renaming preserves aliases and GUID commands. After deletion, `--game` or `--script` fails clearly and non-interactively; it never redirects to another environment.

## Commands and Shortcuts

The canonical activation interface is:

```text
RenderNorthDisplaySwitcher.exe --environment-id <guid>
```

Name activation is a convenience and must reject missing or ambiguous names. Compatibility commands `--game` and `--script` resolve only their legacy aliases. No arguments open the GUI. Silent activation does not initialize WinForms and remains possible while the GUI is running.

Generated shortcuts target the stable, non-versioned Velopack root executable and use `--environment-id`. They must never target `current`, a versioned directory, or a mutable environment name. No per-environment executable is generated. Legacy Game and Script launchers and shortcuts remain during v0.4.

## UI Language

The product presents workspaces, not display profiles. The main UI emphasizes Current Environment, environment selection, activation, editing, capture, ordering, and shortcut creation. It uses `Custom Configuration` when the detected system state matches no saved environment.

Internal terms such as module registry and lifecycle are not exposed. Version 0.4 exposes Displays only and must not present nonfunctional Audio, Applications, OBS, Lighting, HDR, AI, or window-layout controls.

## Backward Compatibility and Product Identity

The v0.4 public name is RenderNorth Environments. To preserve the verified v0.3.2 update path, v0.4 temporarily retains:

- package ID `RenderNorth.DisplaySwitcher`;
- executable `RenderNorthDisplaySwitcher.exe`;
- existing install directory;
- existing GitHub repository and update feed;
- existing `UserData` directory;
- legacy launchers, shortcuts, and CLI aliases.

Historical v0.3.x release records remain unchanged. Internal identity changes require a separately designed and installed-artifact-tested migration.

## Rules for Future Capabilities

A new capability must:

- implement the lifecycle contract;
- register through `EnvironmentModuleRegistry`;
- own an independently versioned persistence schema;
- keep implementation details inside its module and service boundary;
- support validation, readiness, result reporting, and rollback expectations;
- participate in sequential activation ordering;
- preserve data it does not own;
- include unit tests and installed integration tests appropriate to its system APIs;
- use user-facing capability language rather than exposing module internals.

Future modules must never access display internals or add capability-specific branches to `EnvironmentManager`.

## Prohibited Architecture Shortcuts

- Treating an environment as a display profile with extra labels.
- Hardcoding Game Mode, Script Mode, or a two-environment limit into new domain logic.
- Using mutable names as persistent identity.
- Adding module-type conditionals to `EnvironmentManager`.
- Storing core domain fields in an unstructured metadata dictionary.
- Creating empty future module classes or nonfunctional UI.
- Activating modules concurrently in v0.4.
- Dropping unknown module documents during load or save.
- Deleting or overwriting user data without a validated backup.
- Rewriting the native display engine merely to match the abstraction.
- Generating a compiled executable per environment.
- Targeting versioned install directories from shortcuts.
- Assuming development-build behavior proves installed-artifact behavior.
- Publishing, changing package identity, or changing the update feed without explicit owner approval and migration testing.

## Verification Standard

Domain behavior, persistence, migration, commands, orchestration, and shortcut generation require automated tests. Native Windows behavior is verified through installed integration tests where unit tests would be artificial. Builds must remain warning-free, and installed update/migration behavior must be tested rather than assumed.
