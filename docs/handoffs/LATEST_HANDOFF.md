# Latest Handoff

## Milestone

Phase 11 installed migration testing is in progress. The stable shortcut-root correction is implemented and tested locally.

## Features completed

- `ShortcutService.CreateDefault` derives the installed executable from `AppContext.BaseDirectory` when running from Velopack `current`.
- Added a regression test proving an account-dependent Velopack locator root cannot produce the wrong-user shortcut target.
- Built and packaged a private `0.4.0-preview.3` Velopack package.
- Applied the package to the installed `C:\Users\vance\AppData\Local\RenderNorth.DisplaySwitcher` copy.
- Recreated the real Start-menu environment shortcut and verified its target is the stable root executable with the permanent environment GUID.

## Files changed

- `Services/ShortcutService.cs`
- `Tests/ShortcutServiceTests.cs`
- `docs/handoffs/LATEST_HANDOFF.md`

## Tests and commands actually run

- `build.ps1` — passed; 43 tests passed, 0 warnings, 0 errors.
- Private publish/package commands for `0.4.0-preview.3` — completed; Velopack setup, portable ZIP, and full package created.
- Velopack `Update.exe apply --norestart --package ...preview.3-full.nupkg` — installed preview.3 over preview.2.
- WScript shortcut inspection — target is `C:\Users\vance\AppData\Local\RenderNorth.DisplaySwitcher\RenderNorthDisplaySwitcher.exe`; arguments are `--environment-id f5637a42-5c46-4584-b511-5e0cd1a2f758`.
- Real shortcut launch — process returned without a popup; display activation logged Windows error 87 and left the environment inactive.

## Build and launch status

Build and automated tests pass. The installed preview launches. Stable shortcut generation is corrected. Physical display activation from the recreated shortcut requires diagnosis because this attempt returned `SetDisplayConfig` error 87.

## Commit

`9050fe2` — Fix stable installed shortcut root

## Known issues

- Installed shortcut activation has an observed Windows error 87 in the current machine state. The existing display engine was not modified.
- Physical Stream Deck button verification remains an owner action.
- Rename/update/uninstall evidence is not yet complete for this resumed test run.

## Next milestone

Diagnose the installed error-87 state without changing the native display engine, rerun activation from the opposite known-good layout, then complete rename, restart, update, and uninstall checks.

## Owner gates

- Owner must physically press the Stream Deck buttons and confirm the capture output.
- Public versioning, tagging, pushing, publishing, or changing the update feed remains gated.

## Architecture review

External ChatGPT architecture review is not genuinely required at this point. The approved architecture remains consistent; escalation is needed only if installed testing reveals a migration or product-direction conflict.
