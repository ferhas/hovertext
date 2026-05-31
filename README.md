# HoverText

<div align="center">

**Floating text for Windows accessibility.**

HoverText helps people with low vision read small or hard-to-focus text on Windows by showing a large, high-contrast floating view near the cursor.

[Read this in Chinese](README.zh-CN.md)

![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Accessibility](https://img.shields.io/badge/Accessibility-Low%20Vision-2E7D32?style=for-the-badge)

</div>

---

## Why HoverText exists

Apple platforms include a useful accessibility feature called **Hover Text**: point at text, press a key, and read a larger version without losing context.

Windows users deserve the same kind of lightweight help. HoverText is built to fill that gap on Windows: it brings cursor-following, high-contrast floating text to everyday apps without forcing a full-screen magnifier or changing how people work.

## What it does

- Hold a trigger key to read text under the cursor; the default trigger is `Alt`
- Shows a large, high-contrast, always-on-top floating text window near the cursor
- Uses Windows UI Automation first for reliable control text
- Falls back to optional Tesseract OCR when UI Automation cannot provide text
- Falls back again to a 3x pixel magnifier around the cursor when OCR is unavailable or fails
- Shows tooltip-like text for icon-only controls instead of jumping straight into magnifier mode
- Provides a Hover Typing-style editable enlarged input box while typing
- Shrinks long text automatically to reduce clipping
- Avoids screen edges and works across common browsers, editors, Explorer, and desktop apps
- Runs from the system tray and stores settings in `%APPDATA%\HoverText\settings.json`

## Who it is for

- Windows users with low vision or difficulty reading small text
- People who spend long hours reading dense screens
- Users who need quick local enlargement without turning on a full-screen magnifier
- Anyone who wants an Apple Hover Text-like experience on Windows

## Run

```powershell
.\.dotnet\dotnet.exe run --project .\src\HoverText.App\HoverText.App.csproj
```

Or run the built executable:

```powershell
.\src\HoverText.App\bin\Debug\net8.0-windows\HoverText.App.exe
```

## Test

```powershell
.\.dotnet\dotnet.exe test .\HoverText.sln
```

## OCR

OCR support is optional and uses the Tesseract CLI. If `tesseract.exe` is available in `PATH`, or installed at `C:\Program Files\Tesseract-OCR\tesseract.exe`, HoverText will enable the OCR fallback automatically. Otherwise, it falls back to pixel magnification.

## Project Status

HoverText is an early Windows accessibility utility. The core experience is already usable, and the next milestones are packaging, broader app compatibility, and smoother installation.

Contributions, issue reports, and accessibility feedback are welcome.
