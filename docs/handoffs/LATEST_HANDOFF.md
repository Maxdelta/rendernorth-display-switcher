# Latest Handoff

## Milestone

Phase 11 installed migration testing is complete. The stable shortcut-root correction and physical Stream Deck verification are complete. v0.4.0 remains unpublished pending explicit approval.

## Features completed

- `ShortcutService.CreateDefault` derives the installed executable from `AppContext.BaseDirectory` when running from Velopack `current`.
- Added a regression test proving an account-dependent Velopack locator root cannot produce the wrong-user shortcut target.
- Built and applied a private `0.4.0-preview.3` Velopack package to the installed copy.
- Recreated the real Start-menu environment shortcut and verified its target is the stable root executable with the permanent environment GUID.
- Manually restored and recaptured both known-good layouts after the stale adapter-LUID failure.
- Verified Game Mode and Script Mode from the application and from the physical Stream Deck.
- Verified the Elgato output changes correctly for both environments.
- Verified recapture preserves environment identity and shortcut compatibility.
- Verified failed stale configurations were not falsely marked active and existing rollback/validation behavior remained intact.

## Files changed

- `Services/ShortcutService.cs`
- `Tests/ShortcutServiceTests.cs`
- `docs/handoffs/LATEST_HANDOFF.md`

## Tests and commands actually run

- `build.ps1` — passed; 43 tests passed, 0 warnings, 0 errors.
- Private publish/package commands for `0.4.0-preview.3` — completed.
- Velopack `Update.exe apply --norestart --package ...preview.3-full.nupkg` — installed preview.3 over preview.2.
- WScript shortcut inspection — target is `C:\Users\vance\AppData\Local\RenderNorth.DisplaySwitcher\RenderNorthDisplaySwitcher.exe`; arguments use the permanent environment GUID.
- Real shortcut launch — stable root target and GUID activation were verified silently.
- Owner physical verification — Game Mode and Script Mode both worked from Stream Deck and changed the Elgato output correctly.

## Build and launch status

Build and automated tests pass. The installed preview launches. Stable shortcut generation, GUID activation, silent operation, recaptured environments, and physical Stream Deck integration are verified.

## Commit

`9050fe2` — Fix stable installed shortcut root

## Known issues

- Previously saved stale configurations can fail with Windows error 87 after adapter re-enumeration. Manually restoring and recapturing the known-good layouts resolved the issue without an engine change.
- v0.4.0 has not been published.

## Next milestone

Prepare a release-readiness review for v0.4.0 and request explicit owner approval before versioning, tagging, pushing, publishing, or changing the update feed.

## Owner gates

- Public versioning, tagging, pushing, publishing, or changing the update feed remains gated.

## Architecture review

External ChatGPT architecture review is not genuinely required at this point. The approved architecture remains consistent.

## RN-014 Productization Review

The approved RN-014 Productization Phase brief has been reconciled against the current implementation. The Environment Manager UI, multi-environment domain, editor/capture flow, categories/icons, GUID shortcuts, silent activation, migration compatibility, and RenderNorth branding are implemented through the existing Phases 1–10 commits. No future capability modules were added.

Verification on this review:

- `build.ps1` — passed; 43 tests passed, 0 warnings, 0 errors.
- Working tree — clean before this handoff update.
- v0.4.0 — not published; package identity, executable name, install directory, updater identity, repository, and feed were not changed.

Next product gate: release-readiness review and explicit owner approval before versioning, tagging, pushing, publishing, or changing the update feed.

## RN-014 Final Product Polish

- Replaced the misleading Settings-to-About behavior with a lightweight Settings window covering General, Updates, Logging, Diagnostics, Shortcut Management, and About.
- Added activation busy-state protection by disabling the environment list during activation and restoring it afterward.
- Replaced placeholder environment text labels with a consistent built-in glyph mapping.
- Added an explicit managed-shortcut cleanup choice to environment deletion; unmanaged shortcuts are never touched.
- Build/test verification: `build.ps1` passed with 43 tests, 0 warnings, and 0 errors.
- No display-engine, architecture, package identity, updater, version, or release-feed changes were made.
