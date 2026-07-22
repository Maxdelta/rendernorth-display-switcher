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
- [ ] Add a real Stream Deck configuration screenshot or short switching demo GIF to the public README

## RenderNorth Environments v0.4.0

### Phase 1 - Architecture documentation

- [x] Define the modular Environment architecture and lifecycle
- [x] Record compatibility, transaction, persistence, and unknown-module rules
- [x] Establish the approved implementation order

### Phase 2 - Test foundation

- [x] Add a dedicated automated test project
- [x] Add domain-level fixtures for legacy profiles and module documents
- [x] Establish baseline compatibility tests

### Phase 3 - Domain and persistence

- [x] Add Environment and EnvironmentCollection
- [x] Add ModuleDocument and independent schema versions
- [x] Add IEnvironmentModule and EnvironmentModuleRegistry
- [x] Add atomic EnvironmentRepository with unknown-module preservation

### Phase 4 - Display extraction

- [x] Add DisplayModule and DisplayModuleService
- [x] Move display orchestration without changing native behavior
- [x] Preserve validation, detection, verification, and rollback

### Phase 5 - Migration

- [x] Add backup-first LegacyMigrationService
- [x] Convert legacy Game Mode and Script Mode profiles without changing payloads
- [x] Verify migration idempotency and preserve legacy files

### Phase 6 - Environment orchestration

- [x] Add module-agnostic EnvironmentManager lifecycle orchestration
- [x] Add reverse-order rollback and activation status persistence
- [x] Add create, rename, duplicate, delete, reorder, capture, activate, and detect

### Phase 7 - CLI and compatibility

- [x] Add structured ID and name activation commands
- [x] Preserve `--game`, `--script`, and compatibility launchers
- [x] Preserve silent activation while the GUI is running

### Phase 8 - UI

- [x] Replace fixed mode cards with the environment workspace UI
- [x] Add environment editor and current-environment status
- [x] Expose the Displays capability without internal module terminology

### Phase 9 - Shortcut integration

- [x] Add stable GUID-based Desktop, Start menu, and custom shortcuts
- [x] Add copy, open, recreate, and delete shortcut actions
- [x] Preserve legacy Game and Script shortcuts

### Phase 10 - Branding and documentation

- [x] Update current product-facing language to RenderNorth Environments
- [x] Document temporary compatibility identities
- [x] Preserve historical v0.3.x release records

### Phase 11 - Installed migration testing

- [ ] Test public v0.3.2 to local v0.4 migration
- [ ] Verify backups, legacy launchers, ID activation, rename-safe shortcuts, restart, update, and uninstall
- [ ] Record physical Stream Deck testing only after owner verification

### Phase 12 - Release preparation

- [ ] Stop and request owner approval before versioning, tagging, pushing, publishing, or changing the update feed

## v0.3.0

- [x] Add polished RenderNorth GUI and profile cards
- [x] Add authoritative semantic version source
- [x] Integrate Velopack startup and update service
- [x] Keep update work out of silent switching modes
- [x] Add installer/update/portable packaging and GitHub Actions workflow
- [x] Add release instructions and Stream Deck icons
- [x] Configure the public GitHub update source for `Maxdelta/rendernorth-display-switcher`
- [ ] Test a real GitHub update from an older installed release
- [ ] Add Authenticode signing certificate
