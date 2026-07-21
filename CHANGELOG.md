# Changelog

All notable changes to RenderNorth Display Switcher are documented here.

The project follows [Semantic Versioning](https://semver.org/).

## [0.2.0] - 2026-07-21

### Added

- Dedicated Game Mode and Script Mode Stream Deck launcher executables.
- Fully silent automatic switching through `--game` and `--script`.
- Configuration-derived current-profile status in the GUI.
- Persistent last-switch result and successful-switch timestamp.
- Rollback protection, post-apply topology verification, and local logging.
- Public-release documentation and packaging.

### Verified

- Game Mode and Script Mode switching on the original three-display Windows 11 setup.
- Silent direct commands and both launcher workflows on the original target machine.

## [0.1.0] - 2026-07-21

### Added

- Initial Windows Forms interface.
- Native Windows display configuration capture and restoration.
- Saved Game Mode and Script Mode profiles.
