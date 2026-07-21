# Release Quality Review

## v0.3.0 public release

Reviewed against the RenderNorth Desktop Product Standard on July 21, 2026.

### Public repository

- Description clearly positions the dual-PC capture-card use case.
- Website points to `https://rendernorth.com`.
- Topics cover Windows, .NET, C#, streaming, dual-PC, Elgato, capture cards, desktop apps, monitor management, and display switching.
- MIT license, contribution guide, security policy, release workflow, and public README are present.
- A 1280×640 GitHub social preview is stored at `assets/repository/rendernorth-display-switcher-social-preview.png` and is ready for manual upload in repository settings.

### Release assets

| Asset | Purpose | Review |
|---|---|---|
| `RenderNorth.DisplaySwitcher-win-Setup.exe` | Installed edition | Clear product, platform, and package type |
| `RenderNorth.DisplaySwitcher-win-Portable.zip` | Portable edition | Clear product, platform, and package type |
| `RenderNorth.DisplaySwitcher-0.3.0-full.nupkg` | Velopack update package | Framework-standard, versioned name |
| `RELEASES` | Velopack compatibility feed | Framework-required name |
| `releases.win.json` | Windows update feed | Framework-standard platform name |

The release workflow also produces checksums and packages the license, quick start, README, Stream Deck launchers, icons, and required runtime files. Published v0.3.0 release notes now explain download choices, verification scope, SmartScreen expectations, and the unverified end-to-end updater boundary.

### UI consistency review

The main window follows a consistent 560-pixel content grid, six-pixel card rhythm, aligned action controls, restrained Segoe UI typography, and a clear hierarchy from active profile through status. Game and Script actions are visually distinct without changing their behavior. Status labels use consistent prefixes and practical fallback wording.

The About dialog uses the authoritative application version and consistently presents the product name, RenderNorth creator line, GitHub, website, MIT license, copyright, update action, and close action. No source change was needed for this documentation-only review.

### Remaining release-quality opportunities

- Upload the prepared social preview through GitHub repository settings.
- Add a real Stream Deck configuration screenshot or user-recorded switching demonstration.
- Add Authenticode signing to reduce SmartScreen friction.
- Complete and document an installed older-version-to-newer-public-release updater test before claiming end-to-end updater verification.
