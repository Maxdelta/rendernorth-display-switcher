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
