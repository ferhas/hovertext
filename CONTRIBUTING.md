# Contributing to HoverText

Thank you for helping improve HoverText. The project is especially interested in accessibility feedback from Windows users with low vision, assistive-technology users, and developers who can test across different Windows apps.

## Useful contributions

- Report apps where HoverText cannot read text reliably.
- Share low-vision accessibility use cases and usability feedback.
- Improve Windows UI Automation coverage.
- Improve OCR fallback behavior.
- Improve packaging, signing, and installer workflows.
- Add focused tests for text probing, overlay placement, sizing, and settings behavior.

## Development setup

Build and test from the repository root:

```powershell
.\.dotnet\dotnet.exe test .\HoverText.sln
```

Run the app from source:

```powershell
.\.dotnet\dotnet.exe run --project .\src\HoverText.App\HoverText.App.csproj
```

## Pull request checklist

- Keep changes focused on one behavior or documentation area.
- Add or update tests when changing logic in `src\HoverText.Core` or app behavior that can be tested without a live desktop session.
- Describe the Windows version and apps you used for manual testing when a change affects UI Automation, OCR, keyboard input, tray behavior, or overlay behavior.
- Avoid committing generated build outputs from `bin`, `obj`, `.dotnet`, `TestResults`, or `artifacts`.

## Accessibility feedback

When reporting accessibility feedback, please include:

- Windows version and display scaling.
- The app or screen where the issue happens.
- Whether the text is normal UI text, browser content, an icon tooltip, or image-based text.
- What HoverText showed instead.
- Any privacy-sensitive text redacted from screenshots or logs.

## Code of conduct

Please keep discussions practical, respectful, and centered on helping users read and work more comfortably on Windows.
