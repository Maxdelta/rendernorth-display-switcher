# RenderNorth Display Switcher Website Handoff

Temporary implementation handoff for the RenderNorth website project. This document is product-local and is not authoritative Memory Engine content.

## 1. Product Identity

- **Product name:** RenderNorth Display Switcher
- **RN number:** RN-014
- **Current version:** v0.3.1
- **Status:** Public Release
- **Platform:** Windows 11 x64
- **Technology:** C# and .NET 8
- **License:** MIT

## 2. Core Product Message

### Headline

One-click display switching for dual-PC creators.

### Subheadline

Switch which monitor is mirrored to your capture card without opening Windows Display Settings during a live stream.

### Elevator pitch

RenderNorth Display Switcher is a lightweight Windows utility for dual-PC streamers, presenters, and creators. It saves two known-good Windows display layouts and switches between them instantly from the application or an Elgato Stream Deck.

## 3. Real Use Case

### Game Mode

- The main gaming monitor is duplicated to the capture card.
- The game stays directly in front of the creator.
- The second monitor remains extended.

### Script Mode

- The main monitor remains private for a script beneath the camera.
- The second monitor is duplicated to the capture card.
- Viewers see gameplay, files, code, or browser content from the second monitor.
- The streaming computer does not receive the private script display.

## 4. Key Features

- One-click Game Mode and Script Mode
- Silent Elgato Stream Deck controls
- Stable installed Stream Deck shortcuts
- Native Windows display-profile restoration
- Active-profile detection
- Local logging
- Installer and portable editions
- In-app updates for the installed edition
- Public v0.3.0-to-v0.3.1 update path successfully tested
- No OBS, Streamlabs, or DisplayFusion required on the gaming PC
- Local-first operation
- No account required
- No analytics or credential collection

The primary verified hardware setup uses an Elgato HD60 X. Broader capture-card compatibility is not claimed.

## 5. Download Information

### Recommended download

`RenderNorth-Display-Switcher-Setup-v0.3.1.exe`

- The Setup edition is recommended for normal users and supports in-app updates.
- The portable edition is intended for advanced or testing use and requires manual replacement when updating.
- Velopack `.nupkg`, `RELEASES`, and `releases.win.json` files support the updater and are not normal user downloads.

- **Public release:** https://github.com/Maxdelta/rendernorth-display-switcher/releases/tag/v0.3.1
- **Repository:** https://github.com/Maxdelta/rendernorth-display-switcher

## 6. System Requirements

- Windows 11 x64
- Compatible multi-monitor Windows setup
- HDMI capture card for the intended dual-PC use case
- Elgato Stream Deck optional
- The current per-user Setup installer normally does not require administrator rights; Windows may request approval where required by local security policy

No minimum CPU, memory, or GPU specification has been established.

## 7. Installation and First Run

1. Download `RenderNorth-Display-Switcher-Setup-v0.3.1.exe`.
2. Install RenderNorth Display Switcher.
3. Launch **RenderNorth Display Switcher** from the Windows Start menu.
4. Configure the desired gaming layout in Windows Display Settings and save Game Mode.
5. Configure the private-script layout and save Script Mode.
6. Optionally connect the installed Game and Script shortcuts to Stream Deck.
7. Use **Check for Updates** for future installed versions.

Test both saved layouts while watching the capture preview before using them during a live production.

## 8. Stream Deck Setup

Connect Stream Deck to the main or gaming PC where RenderNorth Display Switcher is installed.

1. In Stream Deck, choose **System → Open Application**.
2. Select **RenderNorth Display Switcher - Game Mode**.
3. Add another **System → Open Application** action.
4. Select **RenderNorth Display Switcher - Script Mode**.

The installed shortcuts are stable across updates and avoid development, artifacts, build-output, temporary, or version-numbered paths.

## 9. Website Calls to Action

- **Primary:** Download for Windows
- **Secondary:** View on GitHub
- **Additional:** Read Setup Guide

## 10. Known Limitations

- Windows-only
- Exactly two saved profiles: Game Mode and Script Mode
- Unsigned binaries may trigger Windows SmartScreen
- A brief black screen may occur while Windows and the capture card renegotiate the signal
- Broader capture-card compatibility has not been fully verified

## 11. Product Story

I built RenderNorth Display Switcher because I use a dual-PC streaming setup. During gameplay, I want the game on the monitor directly in front of me. When reading a script beneath my camera, I want that monitor to remain private while viewers continue seeing content from my second monitor. Windows made changing that routing awkward during a live stream, so I built a one-button solution.

## 12. Website Metadata

- **SEO title:** RenderNorth Display Switcher | One-Click Dual-PC Display Switching
- **Meta description:** Switch which monitor reaches your capture card with one click. A Windows 11 utility for dual-PC creators with silent Stream Deck controls.
- **Open Graph title:** RenderNorth Display Switcher
- **Open Graph description:** One-click monitor routing for dual-PC creators, with saved Game and Script modes and silent Stream Deck controls.
- **Suggested URL slug:** `/products/display-switcher`
- **Suggested product category:** Creator Tools
- **Suggested status label:** Public Release
- **Suggested version label:** v0.3.1

## 13. Frequently Asked Questions

### Does this require OBS?

No. It changes the Windows display topology on the gaming PC and does not control OBS.

### Does this require Streamlabs on the gaming PC?

No. Streamlabs is not required on the gaming PC.

### Does it work with Stream Deck?

Yes. The installed edition creates stable, silent Game Mode and Script Mode shortcuts for Stream Deck's Open Application action.

### Is an Elgato HD60 X required?

The HD60 X is the primary verified capture device. Other capture devices may work when Windows recognizes them as display outputs, but broader compatibility is not fully verified.

### Does it support automatic updates?

The installed edition can check the public GitHub Release source, download an available update, and restart to install it. The public v0.3.0-to-v0.3.1 path was tested successfully.

### Does the portable version update automatically?

No. Portable installations must be replaced manually.

### Will switching briefly interrupt the capture feed?

It may. Windows and the capture card can briefly renegotiate the signal, producing a short black screen.

### Is the application free?

Yes.

### Is it open source?

Yes. The source is available under the MIT License.

### Does it collect data?

No accounts, analytics, advertising, telemetry, or credentials are collected. Profiles and logs remain local. The installed edition contacts the public GitHub Release source for update information and packages.

## 14. Assets Inventory

### Available repository assets

- **Main application screenshot:** `assets/screenshots/application-main-window.png`
- **Game Mode Stream Deck icon:** `assets/stream-deck/game-mode.png`
- **Script Mode Stream Deck icon:** `assets/stream-deck/script-mode.png`
- **Repository social preview/banner:** `assets/repository/rendernorth-display-switcher-social-preview.png`
- **Installer:** `artifacts/velopack/RenderNorth-Display-Switcher-Setup-v0.3.1.exe`
- **Portable ZIP:** `artifacts/velopack/RenderNorth-Display-Switcher-v0.3.1-win-x64-Portable-Manual-Updates.zip`
- **README:** `README.md`
- **First-run guide:** `docs/FIRST_RUN.md`
- **Release notes:** `docs/releases/v0.3.1.md`

The Windows display screenshots under `assets/screenshots` are approved public documentation assets; `windows-game-mode.png` has been sanitized to remove personal identity information.

### Missing website assets

- Standalone application logo/icon
- Product demo GIF or video
- Full Stream Deck photograph
- Dedicated website hero banner distinct from the repository social preview
- Secondary product-workflow screenshot designed specifically for the website

## Verified Links

- **Repository:** https://github.com/Maxdelta/rendernorth-display-switcher
- **v0.3.1 release:** https://github.com/Maxdelta/rendernorth-display-switcher/releases/tag/v0.3.1
- **Recommended installer:** https://github.com/Maxdelta/rendernorth-display-switcher/releases/download/v0.3.1/RenderNorth-Display-Switcher-Setup-v0.3.1.exe
