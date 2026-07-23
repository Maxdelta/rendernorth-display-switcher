# RenderNorth Environments

![Windows 11](https://img.shields.io/badge/Windows-11-0078D4?logo=windows11&logoColor=white)
[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![MIT License](https://img.shields.io/github/license/Maxdelta/rendernorth-display-switcher)](LICENSE)
[![Latest Release](https://img.shields.io/github/v/release/Maxdelta/rendernorth-display-switcher)](https://github.com/Maxdelta/rendernorth-display-switcher/releases/latest)

**Your PC should adapt to what you are doing.**

RenderNorth Environments is a Windows desktop workspace launcher. Save complete environments for gaming, streaming, development, editing, presentations, travel, or work, then activate the right workspace from the application, a command, or Stream Deck.

The v0.4 preview implements the first environment capability: **Displays**. It captures and restores native Windows display topology, including active displays, primary display, duplicate and extend relationships, positions, resolutions, and supported refresh information.

## Early Access Status

RenderNorth Environments v0.4.0 is an Early Access build prepared for owner review. It is not yet a public release. Existing users should continue using their current installed release until v0.4.0 receives explicit publication approval.

## What Is an Environment?

An Environment is a named workspace with a permanent ID and one or more capabilities. Names are editable; IDs keep commands and shortcuts stable.

Examples:

- Gaming
- Streaming
- Development
- Editing
- Presentation
- Work
- Travel

Displays are only the first capability. The internal architecture can later support Audio, Applications, Window Layout, OBS, Lighting, and other workspace automation without placing those concerns inside the display engine. These future capabilities are not exposed in v0.4.

See [Architecture Vision](docs/ARCHITECTURE_VISION.md) for the permanent module, transaction, data-safety, and compatibility rules.

## Features

- Create, rename, duplicate, delete, favorite, and reorder practical numbers of environments.
- Attach an exact captured Windows display configuration to each environment.
- Detect the active environment from the current Windows topology when possible.
- Display **Custom Configuration** when no saved environment matches.
- Activate modules sequentially through a module-agnostic transaction pipeline.
- Capture rollback state before display changes and reverse changed modules after failure.
- Preserve stable Windows monitor device identities instead of trusting display numbers.
- Generate update-safe shortcuts using permanent environment GUIDs.
- Keep legacy Game Mode and Script Mode profiles, commands, launchers, and shortcuts during migration.
- Store environments and logs locally without analytics, telemetry, advertising, or accounts.

## First Run for the v0.4 Preview

1. Update an existing installed v0.3.2 copy using the approved migration package, or install the local test package.
2. Open **RenderNorth Environments**.
3. Confirm legacy Game Mode and Script Mode profiles appear as environments when applicable.
4. Select **Capture Current Setup** to create another environment from the active Windows display arrangement.
5. Give the environment a unique name, optional description, built-in icon, and category.
6. Save, activate, and verify the detected Current Environment.
7. Use the environment menu to create a Desktop, Start menu, or custom shortcut.

## Environment Shortcuts and Stream Deck

Generated shortcuts call the stable installed executable with a permanent GUID:

```text
RenderNorthDisplaySwitcher.exe --environment-id <guid>
```

They do not target `current`, a version-numbered directory, or a mutable environment name. Renaming an environment therefore does not invalidate its launch command.

For Stream Deck:

1. Open the environment’s **More** menu.
2. Choose **Create Shortcut → Start Menu** or another stable location.
3. Add Stream Deck’s **System → Open** action.
4. Select the generated `.lnk` shortcut.
5. Test the button while watching the actual displays and capture preview.

The legacy Game and Script launchers remain included for v0.4 compatibility.

## Command Line

```text
RenderNorthDisplaySwitcher.exe --environment-id <guid>
RenderNorthDisplaySwitcher.exe --environment "Development"
RenderNorthDisplaySwitcher.exe --activate-environment <guid>
RenderNorthDisplaySwitcher.exe --list-environments
RenderNorthDisplaySwitcher.exe --show
RenderNorthDisplaySwitcher.exe --game
RenderNorthDisplaySwitcher.exe --script
```

Stable ID activation is canonical. Activation commands are silent by default and do not initialize the GUI. Invalid arguments return exit code 2; missing and ambiguous environments return distinct nonzero codes; activation failures return a nonzero code and are logged.

## Backward Compatibility

The v0.4 migration temporarily retains these technical identities so public v0.3.2 installations can update in place:

- Package ID: `RenderNorth.DisplaySwitcher`
- Executable: `RenderNorthDisplaySwitcher.exe`
- Existing install directory and `UserData` directory
- Repository and GitHub update feed
- `--game` and `--script`
- `RenderNorthGameMode.exe` and `RenderNorthScriptMode.exe`

Legacy `game.json` and `script.json` files are backed up and migrated idempotently into Game Mode and Script Mode environments. The original files are not deleted. Legacy aliases remain separate from mutable names.

## Native Display Safety

The Displays capability uses `QueryDisplayConfig`, `DisplayConfigGetDeviceInfo`, and `SetDisplayConfig`. Before activation it validates the saved machine and connected monitor identities. It captures rollback state, applies the supplied paths and modes, and verifies the resulting source-to-target topology.

The proven native engine remains isolated behind `DisplayModuleService`; environment orchestration, persistence, UI, and future capabilities do not access its native structures.

## Building and Testing

Requirements:

- Windows 11 x64
- .NET 8 SDK

```powershell
git clone https://github.com/Maxdelta/rendernorth-display-switcher.git
cd rendernorth-display-switcher
.\build.ps1
```

The build script compiles the main application and compatibility launchers, then runs the dedicated xUnit test project. Builds must remain warning-free.

Publishing and release packaging remain intentionally unchanged until the v0.4 installed migration path is approved. See [Releasing](docs/RELEASING.md).

## Data and Privacy

Installed data is stored outside the installer-managed application directory under `%LOCALAPPDATA%\RenderNorth\Environments\UserData`; portable data remains beside the portable executable. Existing installed `UserData` is migrated automatically. Environment configuration can contain the local machine name and Windows monitor device paths.

RenderNorth Environments has no telemetry, advertising, account system, or analytics. The installed edition contacts the configured public GitHub Releases repository only for optional update checks and packages.

## Known Preview Limitations

- Windows 11 x64 only.
- Displays are the only implemented environment capability in v0.4.
- Display environments are tied to the machine and monitor identities on which they were captured.
- Windows may briefly renegotiate monitor or capture-card signals while applying a topology.
- Code signing is not yet configured.
- Physical Stream Deck verification must be performed by the owner on the target setup.

## Contributing and Security

See [CONTRIBUTING.md](CONTRIBUTING.md) and [SECURITY.md](SECURITY.md). Preserve module boundaries, rollback protection, unknown-module data, updater compatibility, and installed-artifact verification.

## License

RenderNorth Environments is available under the [MIT License](LICENSE).

Created by **RenderNorth**.
