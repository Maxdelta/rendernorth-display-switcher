# RenderNorth Environments

## Your PC should adapt to what you are doing.

RenderNorth Environments is a Windows workspace launcher for saving and activating complete working contexts. Move between Gaming, Streaming, Development, Editing, Presentation, Work, Travel, or your own custom environments without rebuilding the workspace manually.

## The first capability: Displays

Version 0.4 captures and restores native Windows display configuration:

- Active and inactive displays
- Primary display
- Duplicate and extend topology
- Display positions
- Resolution and supported refresh information
- Stable monitor device identities
- Rollback and post-activation verification

Displays are the first capability, not the product limit. The modular architecture can later add real Audio, Applications, Window Layout, OBS, Lighting, and other capabilities without rewriting the environment model.

## Built for reliable activation

Each environment has a permanent GUID. Names can change without breaking generated shortcuts. Capability activation is validated, ordered, logged, and treated as a transaction. Failed changes attempt rollback and never falsely mark an environment active.

## Stream Deck ready

Create a Desktop, Start menu, or custom shortcut from any environment. Every shortcut targets the stable installed application and activates the environment by GUID, surviving updates and renames.

## Compatibility

The first preview migrates existing RenderNorth Display Switcher Game Mode and Script Mode data without deleting the originals. Existing commands, launchers, package identity, install location, update feed, and saved-data location remain compatible during the transition.

## System requirements

- Windows 11 x64
- A display arrangement supported by Windows Display Settings
- Optional Elgato Stream Deck for one-button activation

## Privacy

No account, advertising, analytics, or telemetry. Environments and logs stay local. The installed edition contacts GitHub Releases only for optional updates.

## Availability

RenderNorth Environments v0.4 is undergoing local installed migration testing and is not yet publicly released. The current public v0.3.2 installer remains available at [GitHub Releases](https://github.com/Maxdelta/rendernorth-display-switcher/releases/latest).

Created by **RenderNorth** and licensed under MIT.
