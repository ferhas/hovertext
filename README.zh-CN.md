# HoverText：为低视力 Windows 用户带来 macOS Hover Text 式体验

<div align="center">

**macOS 有 Hover Text，Windows 也应该有。**

HoverText 是一个面向视力障碍和低视力 Windows 用户的辅助工具：按住触发键，把鼠标下方看不清的文字放大成高对比度悬浮文字。

[English](README.md)

[下载最新版 Windows EXE](https://github.com/ferhas/hovertext/releases/latest/download/HoverText-win-x64.exe)

[![CI](https://github.com/ferhas/hovertext/actions/workflows/ci.yml/badge.svg)](https://github.com/ferhas/hovertext/actions/workflows/ci.yml)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Accessibility](https://img.shields.io/badge/Accessibility-Low%20Vision-2E7D32?style=for-the-badge)
[![License: MIT](https://img.shields.io/badge/License-MIT-0f766e?style=for-the-badge)](LICENSE)

</div>

---

## 为什么做 HoverText

Apple 系统里有一个很实用的辅助功能：**Hover Text**。用户把鼠标移到文字上，再按下触发键，就能在不离开当前上下文的情况下看到更大的文字。

Windows 用户也需要类似的轻量帮助。HoverText 想补上这个空白：不强迫用户打开全屏放大镜，不改变原本工作流，只在需要时把鼠标附近的文字清楚地托起来。

## 它能做什么

- 按住 `左 Alt` 读取鼠标下方文字；按住 `右 Alt` 启用放大镜模式
- 可选的 Hover Typing 输入放大模式：输入时把当前文本框内容显示成大号悬浮文字
- 按 `Esc` 可临时隐藏输入放大，继续输入新内容后自动恢复
- 在鼠标附近显示大字号、高对比度、置顶的悬浮文字窗口
- 优先使用 Windows UI Automation 读取控件文本
- UI Automation 无法提供文本时，可使用本地 Tesseract OCR 兜底
- OCR 不可用或识别失败时，回退为鼠标附近区域的 3x 像素放大
- 对只有提示文本的图标或按钮，优先显示提示文字
- 长文本会自动缩小字号，尽量减少裁切
- 自动避开屏幕边缘，适配常见浏览器、编辑器、资源管理器和桌面应用
- 常驻系统托盘，设置保存在 `%APPDATA%\HoverText\settings.json`

## 适合谁

- 视力障碍、低视力或阅读小字困难的 Windows 用户
- 长时间阅读密集屏幕内容的人
- 需要临时放大局部文字，但不想开启全屏放大镜的人
- 希望 Windows 拥有类似 Apple Hover Text 体验的人

## 快速运行

普通用户可以直接下载独立 EXE：

```text
https://github.com/ferhas/hovertext/releases/latest/download/HoverText-win-x64.exe
```

开发时可从源码运行：

```powershell
.\.dotnet\dotnet.exe run --project .\src\HoverText.App\HoverText.App.csproj
```

也可以运行已经构建好的程序：

```powershell
.\src\HoverText.App\bin\Debug\net8.0-windows10.0.19041.0\HoverText.App.exe
```

## 测试

```powershell
.\.dotnet\dotnet.exe test .\HoverText.sln
```

仓库也包含 GitHub Actions 工作流，会在 Windows 环境构建并运行测试。

## OCR 说明

OCR 是可选能力，使用本地 Tesseract CLI。如果 `tesseract.exe` 在 `PATH` 中，或安装在默认路径 `C:\Program Files\Tesseract-OCR\tesseract.exe`，HoverText 会自动启用 OCR 兜底；否则会直接使用像素放大兜底。

## 隐私和安全

HoverText 设计为本地 Windows 工具：

- 核心文本读取使用本地 Windows UI Automation API
- 可选 OCR 使用本地 Tesseract CLI
- 像素放大兜底只读取本地屏幕像素
- 核心功能不依赖网络服务

提交包含截图或录屏的问题前，请先阅读 [SECURITY.md](SECURITY.md)，并遮盖私人内容、凭据和个人信息。

## 参与贡献

真实辅助功能反馈非常重要。好的反馈通常包括 Windows 版本、显示缩放、被测试的应用，以及文字类型是普通 UI 文本、网页内容、提示文字还是图片文字。

更多说明见 [CONTRIBUTING.md](CONTRIBUTING.md)、[SUPPORT.md](SUPPORT.md) 和 GitHub issue 模板。

## 路线图

- 改善常见 Windows 应用、浏览器、编辑器和桌面工具兼容性
- 补充低视力使用场景截图和短演示
- 改善发布包、安装体验和代码签名
- 扩展 OCR 兜底测试和配置能力
- 持续优化悬浮窗位置、字号、对比度和触发键体验

## 项目状态

HoverText 目前是一个早期 Windows 辅助功能工具。核心体验已经可用，下一步会继续完善安装发布、更多应用兼容性和真实辅助功能反馈。

欢迎贡献代码、提交 issue，也欢迎提供来自真实辅助功能使用场景的反馈。
