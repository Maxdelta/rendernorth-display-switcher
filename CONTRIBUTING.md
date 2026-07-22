# Contributing

Thank you for helping improve RenderNorth Environments.

## Before opening an issue

- Search existing issues for the same Windows error or display topology.
- Confirm you are using Windows 11 x64.
- Reproduce the problem with all expected displays connected.
- Remove machine names, account names, and monitor device paths from logs before sharing them publicly.

## Development setup

Install the .NET 8 SDK on Windows 11, clone the repository, then run:

```powershell
.\build.ps1
```

Create publish artifacts with:

```powershell
.\publish.ps1
```

## Pull requests

- Keep changes focused and explain the user-visible behavior.
- Do not replace native Windows display APIs with external display-management dependencies.
- Preserve rollback protection, stable monitor identity validation, and silent automatic mode.
- Keep `EnvironmentManager` module-agnostic and preserve unknown module documents.
- Add new capabilities through the registry and lifecycle instead of capability-specific manager branches.
- Run the complete automated suite and test installed artifacts for operating-system integration changes.
- Build all three projects with zero errors before submitting.
- Document any hardware topology used for manual testing. Never describe a display switch as verified solely because compilation succeeded.
- Update `CHANGELOG.md`, tests/checks, and relevant documentation when behavior changes.

## Style

Follow the existing C# and PowerShell style, enable nullable reference checking, avoid third-party packages unless clearly justified, and keep generated profiles, logs, binaries, and artifacts out of commits.

By contributing, you agree that your contribution may be distributed under the MIT License.
