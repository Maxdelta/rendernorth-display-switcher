# Tasks

## MVP

- [x] Capture the current active Windows display configuration
- [x] Save separate Game and Script profiles
- [x] Validate stable monitor device identities before activation
- [x] Restore paths and modes through `SetDisplayConfig`
- [x] Capture rollback state and restore it after failure or verification mismatch
- [x] Provide GUI actions and `--game` / `--script` commands
- [x] Write local logs and clear result dialogs
- [x] Add build and standalone publish scripts
- [x] Add dedicated no-console Game Mode and Script Mode launcher executables
- [x] Make command-line and Stream Deck switching fully headless with logged exit codes
- [x] Add configuration-derived current-profile and persistent switch status to the GUI
- [x] Test Game Mode switching on the target three-display hardware
- [x] Test Script Mode switching on the target three-display hardware
- [x] Test Stream Deck buttons on the target setup

## v0.2.0 public release

- [x] Add public README, MIT license, changelog, contribution guide, and security policy
- [x] Add application and Windows display-setup screenshots
- [x] Add reproducible clean-folder and ZIP packaging
- [x] Preserve the verified display-switching implementation

## Later improvements

- [ ] Add a notification-area mode
- [ ] Add profile export/import with explicit hardware validation
- [ ] Add optional hotkeys
- [ ] Add code signing and an installer if distribution expands
