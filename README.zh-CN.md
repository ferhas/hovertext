# HoverText 中文版

<div align="center">

**让 Windows 也拥有类似 Apple Hover Text 的悬浮文字辅助。**

HoverText 是一个面向视力障碍、低视力和容易视觉疲劳用户的 Windows 悬浮文字放大工具：按住触发键，即可把鼠标下方的文字放大显示在附近。

[English](README.md)

![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Accessibility](https://img.shields.io/badge/Accessibility-Low%20Vision-2E7D32?style=for-the-badge)

</div>

---

## 为什么做 HoverText

Apple 系统里有一个很实用的辅助功能：**Hover Text**。用户只要把鼠标移到文字上，就可以看到放大的文本，对低视力用户、老年用户、近视或长时间看屏幕的人都很有帮助。

但在 Windows 上，这类“轻量、随处可用、跟随鼠标的悬浮文字”体验并不完整。HoverText 想填补这个空白：不改变用户原本的工作流，不强迫全屏放大，也不要求每个应用单独适配，只是在需要时把看不清的文字清楚地托起来。

## 它能做什么

- 按住触发键显示鼠标下文本，默认触发键为 `Alt`
- 在鼠标附近显示大字号、高对比度、置顶且鼠标穿透的悬浮框
- 优先使用 Windows UI Automation 读取控件文本
- UI Automation 失败时，可使用 Tesseract OCR 作为兜底
- OCR 不可用或识别失败时，可回退为鼠标附近区域的 3x 像素放大
- 对只有提示文本的图标或按钮，显示提示文本，而不是直接进入放大镜模式
- 在输入框处于输入状态时，显示类似 Apple Hover Typing 的可编辑放大输入框
- 长文本会自动缩小字号，尽量避免内容显示不全
- 自动避开屏幕边缘，适配日常窗口、浏览器、编辑器、文件管理器等场景
- 常驻系统托盘，设置可保存到 `%APPDATA%\HoverText\settings.json`

## 适合谁

- 视力障碍、低视力或阅读小字困难的 Windows 用户
- 需要长时间看屏幕、容易视觉疲劳的人
- 想临时放大某一处文字，但不想开启全屏放大镜的人
- 希望 Windows 拥有类似 Apple Hover Text / Hover Typing 体验的人

## 快速运行

```powershell
.\.dotnet\dotnet.exe run --project .\src\HoverText.App\HoverText.App.csproj
```

也可以直接运行已经构建好的程序：

```powershell
.\src\HoverText.App\bin\Debug\net8.0-windows\HoverText.App.exe
```

## 测试

```powershell
.\.dotnet\dotnet.exe test .\HoverText.sln
```

## OCR 说明

OCR 使用可选的 Tesseract CLI。如果系统 `PATH` 中存在 `tesseract.exe`，或安装在默认目录 `C:\Program Files\Tesseract-OCR\tesseract.exe`，HoverText 会自动启用 OCR 兜底；否则会直接进入像素放大兜底。

## 项目状态

HoverText 目前是一个早期 Windows 辅助功能工具。核心体验已经可用，下一步会继续完善安装包、更广泛的应用兼容性和更顺滑的安装流程。

欢迎贡献代码、提交 issue，也欢迎提供来自真实辅助功能使用场景的反馈。
