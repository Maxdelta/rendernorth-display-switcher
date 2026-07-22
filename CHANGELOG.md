# Changelog

All notable changes to RenderNorth Display Switcher are documented here.

The project follows [Semantic Versioning](https://semver.org/).

## [0.3.2] - 2026-07-21

### Fixed

- Installed Game Mode and Script Mode launcher executables now work while the normal GUI is open.
- Both launchers resolve `RenderNorthDisplaySwitcher.exe` from their own `AppContext.BaseDirectory` and log startup, exit codes, and exceptions locally.
- Launcher processes remain silent and use direct no-console process startup.

### Preserved

- The verified native display-profile capture, application, validation, and rollback engine is unchanged.

## [0.3.1] - 2026-07-21

### Improved

- Added stable installed Start-menu shortcuts for silent Game Mode and Script Mode Stream Deck actions.
- Made the installer explicitly create the main Start-menu and Desktop shortcuts.
- Moved installed profiles and logs outside Velopack's replaceable application-content directory and added best-effort legacy profile migration.
- Simplified download guidance around the recommended Setup EXE and clearly labeled portable/manual-update edition.
- Added first-run and release documentation for normal Windows users.

### Preserved

- The physically verified native display-switching engine and silent command behavior are unchanged.

## [0.3.0] - 2026-07-21

### Added

- Restrained dark RenderNorth interface with topology-derived active-profile indicators and prioritized mode cards.
- Functional About dialog using the authoritative application version.
- Velopack 1.2.0 installed edition, manual/background update checks, download progress, and restart-to-install flow.
- GitHub Actions release workflow, installer/update assets, portable ZIP, checksums, and Stream Deck icons.

### Preserved

- The physically verified native display-switching, rollback, logging, and silent Stream Deck behavior from v0.2.0.

## [0.2.0] - 2026-07-21

### Added

- Dedicated Game Mode and Script Mode Stream Deck launcher executables.
- Fully silent automatic switching through `--game` and `--script`.
- Configuration-derived current-profile status in the GUI.
- Persistent last-switch result and successful-switch timestamp.
- Rollback protection, post-apply topology verification, and local logging.
- Public-release documentation and packaging.
- About dialog with RenderNorth, version, GitHub, website, and future-update information.

### Verified

- Game Mode and Script Mode switching on the original three-display Windows 11 setup.
- Silent direct commands and both launcher workflows on the original target machine.

## [0.1.0] - 2026-07-21

### Added

- Initial Windows Forms interface.
- Native Windows display configuration capture and restoration.
- Saved Game Mode and Script Mode profiles.
