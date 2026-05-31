# Security Policy

HoverText is a local Windows accessibility utility. It reads text near the cursor through Windows UI Automation, optional local OCR, or a local pixel magnifier fallback.

## Supported versions

Security reports are accepted for the latest public release and the current `main` branch.

## Reporting a vulnerability

Please report security concerns by opening a private security advisory on GitHub if available, or by contacting the maintainer through the GitHub profile linked from this repository.

Please do not include sensitive screenshots, private documents, credentials, or personal information in public issues.

## Privacy expectations

- HoverText is intended to run locally on the user's Windows machine.
- The app does not require a network service for core text probing, OCR, or magnifier behavior.
- Optional Tesseract OCR support uses a local `tesseract.exe` installation when available.
- Bug reports should redact private screen content before sharing screenshots or logs.

## Out of scope

- Issues caused by modified third-party OCR binaries.
- Reports that require access to private documents or accounts.
- Social engineering or physical access scenarios.
