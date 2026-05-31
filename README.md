# HoverText

HoverText 是一个 Windows 悬浮文字放大工具。运行后常驻托盘，按住 `Alt` 时读取鼠标下方文字，并在鼠标附近显示大字号悬浮框。

## 功能

- 按住触发键显示鼠标下文本，默认触发键为 `Alt`
- 优先使用 Windows UI Automation 读取控件文本
- UIA 失败时可选走 Tesseract OCR 兜底
- OCR 不可用或识别失败时显示鼠标附近截图的像素放大兜底
- 悬浮窗口置顶、鼠标穿透，并自动避开屏幕边缘
- 图标或按钮只有提示文本时，会显示提示文本而不是进入放大镜模式
- 输入框处于输入状态时，会显示类似输入放大框的独立样式
- 已运行实例存在时，重复启动会自动退出，避免多个托盘实例
- 默认最小化到右下角托盘图标；双击托盘图标或菜单里的 `Show HoverText` 可打开控制窗口
- 托盘菜单支持打开设置、切换 OCR、切换开机自启和退出
- 设置会保存到 `%APPDATA%\HoverText\settings.json`

## 运行

```powershell
.\.dotnet\dotnet.exe run --project .\src\HoverText.App\HoverText.App.csproj
```

也可以直接运行构建产物：

```powershell
.\src\HoverText.App\bin\Debug\net8.0-windows\HoverText.App.exe
```

## 测试

```powershell
.\.dotnet\dotnet.exe test .\HoverText.sln
```

## OCR

OCR 使用可选的 Tesseract CLI。如果系统 `PATH` 中存在 `tesseract.exe`，或安装在默认目录 `C:\Program Files\Tesseract-OCR\tesseract.exe`，HoverText 会自动启用 OCR 兜底；否则会直接进入像素放大兜底。
