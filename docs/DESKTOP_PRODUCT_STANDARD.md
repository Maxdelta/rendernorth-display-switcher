# RenderNorth Desktop Product Standard

## Purpose

This standard defines the baseline for polished, supportable RenderNorth desktop software. New applications should inherit these conventions unless a documented product requirement justifies an exception.

## Project layout

- Keep application code, platform interop, services, models, and UI in clearly named folders.
- Store public media under `assets`, separated into screenshots, brand, product-specific integrations, repository artwork, and future media.
- Keep operational documentation under `docs` and repository governance files at the root.
- Exclude profiles, logs, build output, packages, credentials, and machine-specific files from source control.

## Versioning

- Follow Semantic Versioning: `MAJOR.MINOR.PATCH`.
- Maintain one authoritative version source consumed by the application and release automation.
- Display the same version in the About dialog, installer, release title, tag, and packaged metadata.
- Update the changelog before each release.

## Updater

- Make updates optional and explain what will happen before installation.
- Use a public, HTTPS release source and a maintained update framework.
- Never interrupt headless, command-line, automation, or live-production workflows.
- Log update checks and failures without making the core application unavailable.
- Do not claim end-to-end updating is verified until an older installed build successfully upgrades from a newer public release.

## Installer

- Provide a professionally named Windows installer and a clearly labeled portable archive when appropriate.
- Keep install, update, and uninstall behavior testable and documented.
- State whether binaries are code-signed and explain expected SmartScreen behavior honestly.
- Keep user data locations predictable and document whether uninstall removes them.

## GitHub Actions

- Build releases on a clean Windows runner from an exact semantic-version tag.
- Verify the tag matches the authoritative project version.
- build, package, checksum, and publish through one reviewed workflow.
- Use least-privilege workflow permissions and repository-provided secrets.
- Do not manually duplicate a release created by automation.

## Logging

- Write useful local logs with timestamps, operation results, and actionable errors.
- Avoid credentials and unnecessary personal data.
- Document the log location and remind users to redact machine and device identifiers before sharing.
- Logging failures must not hide the original operation result.

## About dialog

- Show the full product name, authoritative version, “Created by RenderNorth,” copyright, license, website, and GitHub links.
- Include Check for Updates only when it performs a real, supported action.
- Use a fixed, centered dialog with consistent spacing and a clear Close action.

## Settings

- Add settings only when users need persistent choices.
- Use safe defaults, plain language, and explicit reset behavior.
- Keep machine-specific configuration local and validate it before applying potentially disruptive changes.

## Release workflow

1. Confirm the intended scope and freeze unrelated behavior.
2. Update the authoritative version, changelog, and user documentation.
3. Run clean builds and automated checks.
4. Perform proportionate manual testing on supported hardware.
5. Produce installer, portable archive, update assets, release notes, and checksums.
6. Review filenames and archive contents.
7. Commit with a clean working tree, tag the exact commit, and push.
8. Inspect the automated release and download every public artifact for a final smoke test.

## Documentation

- Maintain a public README with positioning, screenshots, quick start, installation, updates, troubleshooting, limitations, FAQ, privacy, security, and license.
- Keep `CHANGELOG.md`, `CONTRIBUTING.md`, `SECURITY.md`, release instructions, and product-page copy current.
- Use authentic screenshots only. Verify every relative path, link, and Markdown heading before release.

## Stream Deck support

- When a workflow is intended for Stream Deck, provide dedicated no-console launcher executables that locate the main application relative to themselves.
- Automation must not show windows, popups, splashes, or toast notifications.
- Return meaningful exit codes and write the outcome to the normal log.
- Provide button configuration steps and properly sized icons.

## Branding

- Use the complete product name in public surfaces.
- Present “Created by RenderNorth” consistently and link to the official website and source repository.
- Favor a restrained dark visual system with strong contrast and a limited accent palette.
- Keep product screenshots separate from promotional artwork and never present generated artwork as actual UI.

## UI conventions

- Preserve a clear visual hierarchy: product identity, current state, primary actions, secondary management, utilities, then status.
- Align controls to a consistent grid with predictable padding and button heights.
- Use Segoe UI on Windows, accessible contrast, concise sentence-case status text, and unambiguous destructive or overwrite confirmations.
- Show progress for work that can take time. Keep keyboard acceptance and cancellation behavior consistent in dialogs.
- Avoid redesigning verified workflows solely for visual novelty.

## Error handling

- Fail safely and preserve the prior user state whenever practical.
- Log technical detail while showing concise, actionable language in interactive UI.
- Suppress visible errors in documented headless modes and return non-zero exit codes.
- Never describe hardware-dependent behavior as verified unless it was tested on the stated hardware.

## Release checklist

- [ ] Version is consistent everywhere.
- [ ] Changelog and release notes describe the shipped behavior.
- [ ] Clean build and automated checks pass.
- [ ] Supported hardware workflows are manually tested and accurately reported.
- [ ] Installer, install, update, uninstall, and portable launch are tested as applicable.
- [ ] Headless integrations remain silent.
- [ ] README images, anchors, links, badges, and Markdown are valid.
- [ ] License, contributing, security, privacy, and limitations are current.
- [ ] Artifact names and archive contents are professional.
- [ ] Checksums are published.
- [ ] Repository description, website, topics, and social preview are configured.
- [ ] Release workflow succeeds and public assets download correctly.
- [ ] Working tree is clean and the release commit/tag is pushed.
