# OpenAI Codex for Open Source application notes

This document collects project facts that may be useful when applying for OpenAI Codex for Open Source.

## Repository

https://github.com/ferhas/hovertext

## Maintainer role

Primary maintainer.

## Project summary

HoverText is an open-source Windows accessibility utility for low-vision users. It brings a macOS Hover Text-style experience to Windows: hold a trigger key, point at text, and get a large high-contrast floating text window near the cursor without switching to a full-screen magnifier.

## Why the project matters

Windows includes powerful accessibility tools, but many users still need a lighter, cursor-following way to enlarge just the text they are currently reading. HoverText is designed for people with low vision, users who struggle with dense screens, and anyone who needs temporary local enlargement without losing context.

The project focuses on a practical accessibility gap:

- Local text reading through Windows UI Automation
- Optional local OCR fallback for image-based text
- Pixel magnifier fallback when text extraction is unavailable
- High-contrast floating overlay near the cursor
- System tray workflow and persistent settings

## Current open-source readiness

- Public GitHub repository
- MIT license
- Windows EXE releases
- English and Chinese README files
- Contribution, support, security, and conduct documents
- Bug and accessibility feedback issue templates
- Pull request template
- GitHub Actions CI for Windows build and test
- Focused unit tests for probing, overlay layout, settings, startup policy, and platform helpers

## How Codex/API credits would help

API credits would be used for open-source maintenance work:

- Triage accessibility feedback and convert reports into reproducible compatibility cases
- Review pull requests that touch UI Automation, OCR fallback, overlay placement, settings, and release packaging
- Generate focused regression tests for app compatibility issues reported by users
- Summarize release notes and changelogs from commits and issues
- Assist with documentation for low-vision users and contributors
- Explore safe automation for testing common Windows app scenarios

## Evidence to collect before submitting

- GitHub stars and forks
- Release download counts
- Real issues or accessibility feedback from users
- Screenshots or short demo recordings
- Community posts or discussions mentioning the project
- Examples of apps where HoverText helps or needs compatibility work
