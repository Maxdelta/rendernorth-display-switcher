# RN-014 Project Closeout

## Project registry

| Field | Final value |
| --- | --- |
| Project ID | RN-014 |
| Project name | RenderNorth Environments |
| Working repository | `rendernorth-display-switcher` |
| Public product name | RenderNorth Environments |
| Status | Complete - Private Beta Released |
| Completion date | 2026-07-23 |
| Current version | 0.4.0 |
| Release channel | Private Beta / Prerelease |
| Primary platform | Windows 11 x64 |
| Source commit | `c8bcb3f7a343754cc78525de1d98acadddba9e3d` |
| Release tag | `v0.4.0-private-beta` |
| GitHub repository | https://github.com/Maxdelta/rendernorth-display-switcher |
| Installer | `RenderNorth-Environments-Private-Beta-Setup-v0.4.0.exe` |
| Installer SHA-256 | `B8D816A2A1FEA4490D0BA5C90C4D8B335998A008418D35D6A1154B2A74F5DAB3` |
| GitHub release | https://github.com/Maxdelta/rendernorth-display-switcher/releases/tag/v0.4.0-private-beta |

## Final status

**COMPLETE - PRIVATE BETA RELEASED**

RenderNorth Environments Private Beta v0.4.0 was completed, tested, packaged, published to GitHub, and verified for anonymous Windows download. The release includes environment capture and activation, active-state indication, user-data preservation, installer upgrade/reinstall support, and working Stream Deck integration.

## Verification record

- Release build passed with 0 warnings and 0 errors.
- Automated tests passed: 46/46.
- Install and upgrade passed.
- Uninstall and reinstall passed.
- Intended user data was preserved with an identical SHA-256.
- First launch passed with no startup exception.
- Stream Deck integration worked and managed shortcuts were restored automatically.
- Custom Configuration correctly reflected the active environment and state.
- The GitHub prerelease and installer asset were anonymously downloadable.
- `main` and `codex/rn014-responsive-rebuild` were synchronized with origin at the approved source commit before this documentation-only closeout.

## Known limitations

- The installer is unsigned.
- Windows may display Unknown Publisher or Microsoft Defender SmartScreen warnings.
- Minor visual-polish opportunities remain.
- The repository, executable, and package retain the legacy Display Switcher identity for compatibility.
- Broader public launch and website work are separate tasks.

## Compatibility and recovery

The v0.4.x package ID, executable name, install directory, updater identity, repository, update feed, launchers, Stream Deck shortcuts, and durable user-data location remain compatible with earlier releases. Activation continues to capture rollback state, verify the resulting display topology, and restore the captured configuration when activation fails.

Saved installed data remains under `%LOCALAPPDATA%\RenderNorth\Environments\UserData`, outside the installer-managed application directory.

## Handoff

The next project is **RN-015 - RenderNorth Website - Environments Private Beta Launch**. RN-015 will be handled separately in the website repository. No website work is part of this closeout.
