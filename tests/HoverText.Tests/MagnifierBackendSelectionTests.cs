using HoverText.App.Probing;
using HoverText.Core.Settings;

namespace HoverText.Tests;

[TestClass]
public sealed class MagnifierBackendSelectionTests
{
    [TestMethod]
    public void Factory_creates_gdi_backend_for_default_settings()
    {
        using IMagnifierBackend backend = MagnifierBackendFactory.Create(HoverTextSettings.CreateDefault());

        Assert.AreEqual(MagnifierBackendKind.GdiBitmap, backend.Kind);
        Assert.IsTrue(backend.CanReuseCapture);
    }

    [TestMethod]
    public void Factory_creates_native_windows_backend_when_selected()
    {
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            MagnifierBackend = MagnifierBackendKind.NativeWindows
        };

        using IMagnifierBackend backend = MagnifierBackendFactory.Create(settings);

        Assert.AreEqual(MagnifierBackendKind.NativeWindows, backend.Kind);
        Assert.IsFalse(backend.CanReuseCapture);
    }

    [TestMethod]
    public void Factory_creates_gpu_backend_when_selected()
    {
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            MagnifierBackend = MagnifierBackendKind.GpuDesktopDuplication
        };

        using IMagnifierBackend backend = MagnifierBackendFactory.Create(settings);

        Assert.AreEqual(MagnifierBackendKind.GpuDesktopDuplication, backend.Kind);
        Assert.IsFalse(backend.CanReuseCapture);
    }
}
