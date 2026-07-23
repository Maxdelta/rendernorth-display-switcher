# Decisions

## Capture exact known-good layouts

The MVP stores the active path and mode arrays returned by `QueryDisplayConfig`. This makes Windows Display Settings the configuration UI and avoids guessing which topology or timing combination the GPU driver accepts.

## Stable display identity

Saved profiles record the monitor device path returned by `DisplayConfigGetDeviceInfo`. Activation refuses to proceed if a required device path is absent. Windows UI display numbers are intentionally not stored because they can change.

## Preserve modes and permit driver correction

Saved mode arrays include resolution and refresh timing. `SetDisplayConfig` receives the supplied configuration with `SDC_ALLOW_CHANGES`, allowing Windows to make only necessary driver-supported adjustments.

## Rollback and verification

Activation captures the current layout before changing anything. The resulting source-to-target topology is queried and compared with the saved topology. An API failure or mismatch applies the captured rollback configuration.

## Local, portable storage

Profiles and logs live beside the executable. This keeps the no-installer MVP transparent and easy to back up. Profiles are restricted to the machine on which they were captured.

## Windows Forms

WinForms provides the smallest built-in .NET 8 Windows desktop surface for five buttons and message dialogs, with no third-party packages.

## Dedicated Stream Deck launchers

Two framework-dependent, single-file `WinExe` launchers sit beside the standalone main application. Each resolves the main executable from its own directory, starts the appropriate command, and exits immediately. `WinExe` prevents a console window, while avoiding two additional copies of the large self-contained .NET runtime keeps the launchers small.

## Headless automatic operation

Any argument-based start is treated as Automatic Mode. Recognized profile arguments apply and exit without initializing or displaying the GUI. Invalid arguments, duplicate instances, and switching failures are logged and return non-zero exit codes without message boxes. No-argument startup retains the interactive behavior.

Launcher processes wait for the headless main process and return its exit code. This preserves silent behavior while ensuring Stream Deck activation represents a completed switch rather than only successful process creation.

The public release bundles one shared self-contained .NET runtime beside the two small launcher apphosts. This avoids duplicating the runtime inside both launchers and removes the need for end users to install .NET separately. The main switcher remains a self-contained single file.

## Configuration-derived GUI status

The GUI compares the currently active source-to-target path topology against both saved profiles. It does not infer the current mode from the last button pressed. The last result and successful timestamp are persisted beside the profiles so automatic Stream Deck activity is visible the next time the GUI opens.

## Velopack and GitHub Releases

The installed edition uses Velopack 1.2.0 and the official public GitHub Releases source over HTTPS. Velopack owns feed parsing, package verification, download, apply, restart, install, and uninstall behavior; no custom executable downloader is used. Automatic checks occur only after normal GUI startup. Portable mode reports that automatic updates are unavailable.

`Directory.Build.props` is the single version source. `Services/UpdateService.cs` contains the authoritative public update source: `https://github.com/Maxdelta/rendernorth-display-switcher`.

## RenderNorth Environments modular architecture

The product evolves from RenderNorth Display Switcher into RenderNorth Environments beginning with the planned v0.4.0 preview. An Environment is the primary workspace domain object and owns independently versioned capabilities implemented through `IEnvironmentModule`. Displays are the first capability, not the product boundary.

`EnvironmentManager` orchestrates registered modules exclusively through lifecycle interfaces and generic result contracts. It must remain module-agnostic and activate enabled modules sequentially by `ActivationOrder`. Failed activation stops forward progress and rolls back changed modules in reverse order. Unknown module documents are preserved unchanged.

The existing native display engine moves behind `DisplayModule` and `DisplayModuleService` without a casual rewrite. Legacy Game Mode and Script Mode data migrates backup-first and idempotently into environments retaining their names and the separate aliases `game` and `script`.

For v0.4.0 updater continuity, retain package ID `RenderNorth.DisplaySwitcher`, `RenderNorthDisplaySwitcher.exe`, the install directory, repository, update feed, `UserData` path, and legacy launchers and shortcuts. Public-facing branding becomes RenderNorth Environments. See `docs/ARCHITECTURE_VISION.md` for the governing architecture and prohibited shortcuts.

## RN-014 private beta closeout

RenderNorth Environments is the public product name. The GitHub repository remains `rendernorth-display-switcher`.

For the v0.4.x line, the executable and package identity remain unchanged to preserve installer compatibility, updater compatibility, launcher compatibility, Stream Deck shortcuts, and user-data continuity. Renaming the repository, package, or executable is deferred to a dedicated future migration.

UI polish is frozen for v0.4.0. New feature development is deferred until private-beta feedback is collected. Website launch work is outside RN-014 and is assigned to the separate RN-015 website task.
# v0.3.1 installation decisions

- Use Velopack's standard `Desktop,StartMenuRoot` shortcuts for the normal GUI.
- Create Game and Script Start-menu shortcuts during Velopack install/update hooks. Both target the root-level Velopack execution stub with `--game` or `--script`, providing a stable non-versioned Stream Deck target.
- Keep the dedicated launcher executables in every installed and portable package for compatibility, while recommending Start-menu shortcuts for installed Stream Deck setups.
- Store installed profiles and logs under Velopack's root application directory in `UserData`, outside the replaceable `current` directory. Portable data remains beside the executable.
- Present the friendly Setup EXE and labeled portable archive as the only normal user downloads; retain package/feed assets solely for updater operation.
