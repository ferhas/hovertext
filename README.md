# HoverText: Windows Hover Text, Text Reader, and Cursor Magnifier for Low-Vision Users

<div align="center">

**macOS has Hover Text. Windows should too.**

HoverText is a local Windows accessibility app for visually impaired and low-vision users. It combines Hover Text-style reading, UI Automation text capture, optional OCR, and cursor-following magnification so small text stays readable without switching to a full-screen zoom workflow.

[Read this in Chinese](README.zh-CN.md)

[Download the latest Windows EXE](https://github.com/ferhas/hovertext/releases/latest/download/HoverText-win-x64.exe)

[![CI](https://github.com/ferhas/hovertext/actions/workflows/ci.yml/badge.svg)](https://github.com/ferhas/hovertext/actions/workflows/ci.yml)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Accessibility](https://img.shields.io/badge/Accessibility-Low%20Vision-2E7D32?style=for-the-badge)
[![License: MIT](https://img.shields.io/badge/License-MIT-0f766e?style=for-the-badge)](LICENSE)

</div>

---

## Why HoverText exists

Apple platforms include a useful accessibility feature called **Hover Text**: point at text, press a key, and read a larger version without losing context.

Windows users deserve the same kind of lightweight help. HoverText is built to fill that gap on Windows: it brings cursor-following, high-contrast floating text to everyday apps without forcing a full-screen magnifier or changing how people work.

## What it does

- Hold `Left Alt` to read text under the cursor; hold `Right Alt` for magnifier mode
- Choose between GDI screenshot, native Windows Magnification API, and GPU / DirectX desktop duplication magnifier backends
- Optional Hover Typing mode shows the active text field in large floating text while you type
- Press `Esc` to temporarily hide Hover Typing until the text changes again
- Shows a large, high-contrast, always-on-top floating text window near the cursor
- Uses Windows UI Automation first for reliable control text
- Falls back to optional Tesseract OCR when UI Automation cannot provide text
- Falls back again to a 3x pixel magnifier around the cursor when OCR is unavailable or fails
- Shows tooltip-like text for icon-only controls instead of jumping straight into magnifier mode
- Shrinks long text automatically to reduce clipping
- Avoids screen edges and works across common browsers, editors, Explorer, and desktop apps
- Runs from the system tray and stores settings in `%APPDATA%\HoverText\settings.json`

## Who it is for

- Windows users with low vision or difficulty reading small text
- People who spend long hours reading dense screens
- Users who need quick local enlargement without turning on a full-screen magnifier
- Anyone who wants an Apple Hover Text-like experience on Windows

## Common use cases

- Windows Hover Text alternative for people moving from macOS to Windows
- Low-vision screen magnifier for small UI labels, tooltips, menus, and dense desktop apps
- Cursor text magnifier for reading text near the mouse pointer without changing global display scaling
- Local Windows text reader using UI Automation first, with optional OCR fallback for image-based text
- Lightweight WPF / .NET accessibility utility for Windows 10 and Windows 11

## Run

For most users, download the standalone EXE from the latest release:

```text
https://github.com/ferhas/hovertext/releases/latest/download/HoverText-win-x64.exe
```

For development, run from source:

```powershell
.\.dotnet\dotnet.exe run --project .\src\HoverText.App\HoverText.App.csproj
```

Or run the built executable:

```powershell
.\src\HoverText.App\bin\Debug\net8.0-windows10.0.19041.0\HoverText.App.exe
```

## Test

```powershell
.\.dotnet\dotnet.exe test .\HoverText.sln
```

The repository also includes a GitHub Actions workflow that builds and tests the solution on Windows.

## OCR

OCR support is optional and uses the Tesseract CLI. If `tesseract.exe` is available in `PATH`, or installed at `C:\Program Files\Tesseract-OCR\tesseract.exe`, HoverText will enable the OCR fallback automatically. Otherwise, it falls back to pixel magnification.

## Privacy and security

HoverText is designed as a local Windows utility:

- Core text probing uses local Windows UI Automation APIs
- Optional OCR uses a local Tesseract CLI installation
- The pixel magnifier fallback works from local screen pixels
- The app does not require a network service for its core behavior

Please see [SECURITY.md](SECURITY.md) before sharing bug reports that include screenshots or screen recordings.

## Contributing

Accessibility feedback is especially valuable. Good reports include the Windows version, display scaling, the app being tested, and whether the text was normal UI text, web content, tooltip text, or image-based text.

See [CONTRIBUTING.md](CONTRIBUTING.md), [SUPPORT.md](SUPPORT.md), and the GitHub issue templates for details.

## Roadmap

- Improve compatibility across common Windows apps, browsers, editors, and desktop tools
- Add clearer screenshots and short demo recordings for low-vision use cases
- Improve release packaging, installation, and code signing
- Expand OCR fallback testing and configuration
- Continue refining overlay placement, sizing, contrast, and trigger-key behavior

## Project Status

HoverText is an early Windows accessibility utility. The core experience is already usable, and the next milestones are packaging, broader app compatibility, and smoother installation.

Contributions, issue reports, and accessibility feedback are welcome.
