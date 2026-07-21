# Decisions

## Capture exact known-good layouts

The MVP stores the active path and mode arrays returned by `QueryDisplayConfig`. This makes Windows Display Settings the configuration UI and avoids guessing which topology or timing combination the GPU driver accepts.

## Stable display identity

Saved profiles record the monitor device path returned by `DisplayConfigGetDeviceInfo`. Activation refuses to proceed if a required device path is absent. Windows UI display numbers are intentionally not stored because they can change.

## Preserve modes and permit driver correction

Saved mode arrays include resolution and refresh timing. `SetDisplayConfig` receives the supplied configuration with `SDC_ALLOW_CHANGES`, allowing Windows to make only necessary driver-supported adjustments.

## Rollback and verification

Activation captures the current layout before changing anything. The resulting source-to-target topology is queried and compared with the saved topology. An API failure or mismatch applies the captured rollback configuration.

## Local, portable storage

Profiles and logs live beside the executable. This keeps the no-installer MVP transparent and easy to back up. Profiles are restricted to the machine on which they were captured.

## Windows Forms

WinForms provides the smallest built-in .NET 8 Windows desktop surface for five buttons and message dialogs, with no third-party packages.
