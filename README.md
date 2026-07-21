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

## Using with Elgato Stream Deck

The publish folder includes two dedicated, no-console launcher applications. They locate `RenderNorthDisplaySwitcher.exe` relative to themselves, so the complete publish folder can be moved without editing shortcuts.

1. Open the Stream Deck application.
2. Drag a **System > Open** action onto the button you want to use for Game Mode.
3. In the action's **App / File** field, browse to the publish folder and select `RenderNorthGameMode.exe`.
4. Give the button a title such as **Game Mode**.
5. Drag another **System > Open** action onto the Script Mode button.
6. Select `RenderNorthScriptMode.exe` and title the button **Script Mode**.
7. Press each button once and confirm the main application reports a successful switch.

No command-line argument is needed in Stream Deck. Keep these three files together in the same folder:

- `RenderNorthDisplaySwitcher.exe`
- `RenderNorthGameMode.exe`
- `RenderNorthScriptMode.exe`

The original `--game` and `--script` command-line options remain supported for scripts and terminals.

### Automatic Mode

Starting the main application with `--game` or `--script` runs in Automatic Mode. This mode switches the requested profile without showing a window, splash screen, or message box, then exits as soon as the operation finishes. Failures are written to the local `logs` folder and return a non-zero process exit code without interrupting the screen. The dedicated Stream Deck launchers use Automatic Mode automatically.

Starting `RenderNorthDisplaySwitcher.exe` without arguments continues to open the normal interactive window.

The interactive window includes a status area that compares the active Windows display paths with the saved Game and Script profiles. It reports **Game Mode**, **Script Mode**, or **Custom / Unknown**, along with the latest switch result and time of the last successful switch.

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
