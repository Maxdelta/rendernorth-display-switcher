# RenderNorth Display Switcher

A small Windows 11 utility that captures and restores complete, working display layouts using the native `QueryDisplayConfig` and `SetDisplayConfig` APIs. It does not depend on OBS, Streamlabs, DisplayFusion, or fixed Windows display numbers.

## First-time setup

1. Connect all three displays and the Elgato HD60 X exactly as they will be used.
2. Open Windows **Settings > System > Display** and configure Game Mode:
   - Duplicate the main gaming monitor to the Elgato.
   - Extend the second physical monitor.
   - Keep the main gaming monitor primary.
3. Open `RenderNorthDisplaySwitcher.exe` and choose **Save Current Layout as Game Mode**.
4. Configure Script Mode in Windows Display Settings:
   - Keep the main gaming monitor extended/private.
   - Duplicate the second physical monitor to the Elgato.
   - Keep the main gaming monitor primary.
5. Choose **Save Current Layout as Script Mode**.
6. Test both **Activate** buttons. Profiles are machine-specific and require the same monitor device identities to be connected.

The app captures Windows' exact active paths and modes, including source positions, clone relationships, resolutions, and refresh-rate data. Windows may adjust a mode only when required by the driver or hardware (`SDC_ALLOW_CHANGES`).

## Stream Deck

Create two **System > Open** actions:

- Game: select `RenderNorthDisplaySwitcher.exe` and add argument `--game`
- Script: select `RenderNorthDisplaySwitcher.exe` and add argument `--script`

If your Stream Deck version has one combined App/File field, use a `.bat` shortcut or quote the executable path followed by the argument. Keep the published folder together: profiles and logs are stored beside the executable.

## Build and publish

Requirements: Windows 11 and the .NET 8 SDK.

```powershell
.\build.ps1
.\publish.ps1
```

Build output is under `artifacts\build`. The standalone Windows x64 executable is:

`artifacts\publish\win-x64\RenderNorthDisplaySwitcher.exe`

## Safety and troubleshooting

- Before applying a saved profile, the utility captures the current layout for rollback.
- It verifies connected monitors using stable monitor device paths rather than UI numbers.
- After applying, it verifies the source-to-target topology. A mismatch triggers rollback.
- Logs are written to `logs` beside the executable.
- If rollback itself fails, press `Win+P` or use Windows Display Settings.
- Do not edit profile JSON by hand.

The build can validate interop structure, serialization, and application startup logic. Actual display switching must be tested interactively with this machine's three-display hardware; a successful build is not proof of hardware switching.
