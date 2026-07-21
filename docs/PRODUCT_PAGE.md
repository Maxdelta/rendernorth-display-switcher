# RenderNorth Display Switcher

## One-click monitor switching for dual-PC creators

Move between game capture and a private script display without digging through Windows Display Settings during a live production.

RenderNorth Display Switcher saves two working Windows display layouts and restores either one from its desktop interface or a silent Stream Deck button. It was built around an Elgato HD60 X dual-PC workflow and uses native Windows display APIs—no OBS plugin, Streamlabs integration, or DisplayFusion dependency required.

## Features

- Save exact Game Mode and Script Mode display layouts.
- Switch silently from dedicated Stream Deck launchers.
- Identify monitors by stable device paths rather than changing display numbers.
- Preserve saved display timing data whenever Windows allows it.
- Roll back when an applied layout fails verification.
- See the currently detected profile and last successful switch.
- Keep local diagnostic logs with no accounts, advertising, analytics, or telemetry.
- Install normally with optional updates or use the portable edition.

## Benefits

Stay focused on the stream. Keep scripts and notes private. Route the intended monitor to the capture card with one deliberate action. Reuse the display layouts you already verified in Windows instead of trusting a hardcoded topology.

## Screenshots

### Desktop application

![RenderNorth Display Switcher main window](../assets/screenshots/application-main-window.png)

### Game and Script layouts

| Game Mode | Script Mode |
|---|---|
| ![Game Mode Windows layout](../assets/screenshots/windows-game-mode.png) | ![Script Mode Windows layout](../assets/screenshots/windows-script-mode-setup.png) |

## Download

Download the latest installer or portable Windows x64 archive from the [official GitHub release](https://github.com/Maxdelta/rendernorth-display-switcher/releases/latest).

- **Installer:** recommended for everyday use and optional in-app updates.
- **Portable:** extract and run without installation; update manually.

Unsigned public binaries may trigger Windows SmartScreen. Confirm the download came from the official repository before continuing.

## System requirements

- Windows 11 x64
- A dual-PC or multi-display configuration recognized by Windows
- An HDMI capture device exposed as a Windows display output
- Elgato HD60 X for the originally verified hardware workflow; other models may work but are not individually verified

## Source and documentation

The application is open source under the MIT License. View the [GitHub repository](https://github.com/Maxdelta/rendernorth-display-switcher) for source, setup instructions, release notes, security information, and contribution guidance.

## Frequently asked questions

### Does it control OBS or Streamlabs?

No. It changes the Windows display layout on the gaming PC. Your capture software continues receiving the signal sent to the capture device.

### Will it expose my private script monitor?

Script Mode restores the exact layout you saved. Test both profiles in your capture preview before going live.

### Does Stream Deck switching show a popup?

No. The included Game and Script launcher executables run silently, apply the selected profile, log the result, and exit.

### Are updates forced?

No. Installed builds can check for an update and let you choose whether to download and install it. Portable builds are updated manually.

### Is my data uploaded?

No analytics or telemetry is collected. Profiles and logs stay local. Installed builds contact the public GitHub Releases source over HTTPS for optional update metadata and packages.
