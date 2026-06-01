using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpGen.Runtime;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.App.Probing;

public sealed class GpuDesktopDuplicationMagnifierBackend : IMagnifierBackend
{
    private readonly ScreenMagnifierFallback fallback = new();
    private DesktopDuplicationSession? session;

    public MagnifierBackendKind Kind => MagnifierBackendKind.GpuDesktopDuplication;

    public BitmapSource? LastCapture { get; private set; }

    public bool UsesExternalWindow => false;

    public bool CanReuseCapture => false;

    public async Task<MagnifierResult> CaptureAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        try
        {
            PixelRect captureArea = MagnifierCaptureArea.FromPointer(point);
            DesktopDuplicationSession activeSession = EnsureSession(point);
            PixelRect clippedArea = activeSession.ClipToOutput(captureArea);
            LastCapture = activeSession.Capture(clippedArea);
            return MagnifierResult.Captured(clippedArea, MagnifierDefaults.Scale);
        }
        catch
        {
            session?.Dispose();
            session = null;
            MagnifierResult result = await fallback.CaptureAsync(point, cancellationToken);
            LastCapture = fallback.LastCapture;
            return result;
        }
    }

    public void Dispose()
    {
        session?.Dispose();
        fallback.Dispose();
    }

    public void ShowExternal(PointerPoint cursor, MagnifierResult result, HoverTextSettings settings)
    {
    }

    public void HideExternal()
    {
    }

    private DesktopDuplicationSession EnsureSession(PointerPoint point)
    {
        if (session is not null && session.Contains(point))
        {
            return session;
        }

        session?.Dispose();
        session = DesktopDuplicationSession.Create(point);
        return session;
    }

    private sealed class DesktopDuplicationSession : IDisposable
    {
        private static readonly FeatureLevel[] FeatureLevels =
        [
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0
        ];

        private readonly IDXGIFactory1 factory;
        private readonly IDXGIAdapter1 adapter;
        private readonly IDXGIOutput output;
        private readonly IDXGIOutput1 output1;
        private readonly ID3D11Device device;
        private readonly ID3D11DeviceContext context;
        private readonly IDXGIOutputDuplication duplication;
        private readonly RawRect desktopCoordinates;
        private ID3D11Texture2D? latestFrameTexture;

        private DesktopDuplicationSession(
            IDXGIFactory1 factory,
            IDXGIAdapter1 adapter,
            IDXGIOutput output,
            IDXGIOutput1 output1,
            ID3D11Device device,
            ID3D11DeviceContext context,
            IDXGIOutputDuplication duplication,
            RawRect desktopCoordinates)
        {
            this.factory = factory;
            this.adapter = adapter;
            this.output = output;
            this.output1 = output1;
            this.device = device;
            this.context = context;
            this.duplication = duplication;
            this.desktopCoordinates = desktopCoordinates;
        }

        public static DesktopDuplicationSession Create(PointerPoint point)
        {
            IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            for (uint adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out IDXGIAdapter1? adapter).Success; adapterIndex++)
            {
                for (uint outputIndex = 0; adapter.EnumOutputs(outputIndex, out IDXGIOutput? output).Success; outputIndex++)
                {
                    RawRect rect = output.Description.DesktopCoordinates;
                    if (!Contains(rect, point))
                    {
                        output.Dispose();
                        continue;
                    }

                    IDXGIOutput1 output1 = output.QueryInterface<IDXGIOutput1>();
                    Result result = D3D11.D3D11CreateDevice(
                        adapter,
                        DriverType.Unknown,
                        DeviceCreationFlags.BgraSupport,
                        FeatureLevels,
                        out ID3D11Device? device,
                        out ID3D11DeviceContext? context);
                    result.CheckError();

                    IDXGIOutputDuplication duplication = output1.DuplicateOutput(device);
                    return new DesktopDuplicationSession(factory, adapter, output, output1, device, context, duplication, rect);
                }

                adapter.Dispose();
            }

            factory.Dispose();
            throw new InvalidOperationException("No DXGI output matched the pointer.");
        }

        public bool Contains(PointerPoint point)
        {
            return Contains(desktopCoordinates, point);
        }

        public PixelRect ClipToOutput(PixelRect requested)
        {
            int width = Math.Min(requested.Width, desktopCoordinates.Right - desktopCoordinates.Left);
            int height = Math.Min(requested.Height, desktopCoordinates.Bottom - desktopCoordinates.Top);
            int left = Math.Clamp(requested.Left, desktopCoordinates.Left, desktopCoordinates.Right - width);
            int top = Math.Clamp(requested.Top, desktopCoordinates.Top, desktopCoordinates.Bottom - height);
            return new PixelRect(left, top, width, height);
        }

        public BitmapSource Capture(PixelRect captureArea)
        {
            IDXGIResource? desktopResource = null;
            bool frameAcquired = false;
            try
            {
                uint timeoutMilliseconds = latestFrameTexture is null ? 16u : 0u;
                Result result = duplication.AcquireNextFrame(timeoutMilliseconds, out _, out desktopResource);
                result.CheckError(Vortice.DXGI.ResultCode.WaitTimeout);
                if (result == Vortice.DXGI.ResultCode.WaitTimeout)
                {
                    if (latestFrameTexture is null)
                    {
                        throw new TimeoutException("DXGI desktop duplication did not produce an initial frame in time.");
                    }

                    return CaptureFromTexture(latestFrameTexture, captureArea);
                }

                frameAcquired = true;
                using ID3D11Texture2D sourceTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
                latestFrameTexture ??= CreateLatestFrameTexture(sourceTexture.Description);
                context.CopyResource(latestFrameTexture, sourceTexture);
                return CaptureFromTexture(latestFrameTexture, captureArea);
            }
            finally
            {
                desktopResource?.Dispose();
                if (frameAcquired)
                {
                    duplication.ReleaseFrame();
                }
            }
        }

        public void Dispose()
        {
            latestFrameTexture?.Dispose();
            duplication.Dispose();
            context.Dispose();
            device.Dispose();
            output1.Dispose();
            output.Dispose();
            adapter.Dispose();
            factory.Dispose();
        }

        private static bool Contains(RawRect rect, PointerPoint point)
        {
            return point.X >= rect.Left
                && point.X < rect.Right
                && point.Y >= rect.Top
                && point.Y < rect.Bottom;
        }

        private ID3D11Texture2D CreateLatestFrameTexture(Texture2DDescription sourceDescription)
        {
            var description = new Texture2DDescription(
                sourceDescription.Format,
                sourceDescription.Width,
                sourceDescription.Height,
                1,
                1,
                BindFlags.None,
                ResourceUsage.Default,
                CpuAccessFlags.None,
                1,
                0,
                ResourceOptionFlags.None);

            return device.CreateTexture2D(description);
        }

        private ID3D11Texture2D CreateStagingTexture(PixelRect captureArea)
        {
            var description = new Texture2DDescription(
                Format.B8G8R8A8_UNorm,
                (uint)captureArea.Width,
                (uint)captureArea.Height,
                1,
                1,
                BindFlags.None,
                ResourceUsage.Staging,
                CpuAccessFlags.Read,
                1,
                0,
                ResourceOptionFlags.None);

            return device.CreateTexture2D(description);
        }

        private BitmapSource CaptureFromTexture(ID3D11Texture2D sourceTexture, PixelRect captureArea)
        {
            using ID3D11Texture2D stagingTexture = CreateStagingTexture(captureArea);
            var sourceBox = new Box(
                captureArea.Left - desktopCoordinates.Left,
                captureArea.Top - desktopCoordinates.Top,
                0,
                captureArea.Right - desktopCoordinates.Left,
                captureArea.Bottom - desktopCoordinates.Top,
                1);

            context.CopySubresourceRegion(stagingTexture, 0, 0, 0, 0, sourceTexture, 0, sourceBox);
            return ReadTexture(stagingTexture, captureArea.Width, captureArea.Height);
        }

        private BitmapSource ReadTexture(ID3D11Texture2D texture, int width, int height)
        {
            MappedSubresource mapped = context.Map(texture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
            try
            {
                int stride = width * 4;
                byte[] pixels = new byte[stride * height];
                for (int row = 0; row < height; row++)
                {
                    Marshal.Copy(
                        IntPtr.Add(mapped.DataPointer, row * (int)mapped.RowPitch),
                        pixels,
                        row * stride,
                        stride);
                }

                BitmapSource source = BitmapSource.Create(
                    width,
                    height,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null,
                    pixels,
                    stride);
                source.Freeze();
                return source;
            }
            finally
            {
                context.Unmap(texture, 0);
            }
        }
    }
}
