using System.Runtime.InteropServices;
using OVRSharp;
using Valve.VR;

namespace GameMods.Subnautica.VRCheats;

internal sealed class VrOverlayMenu : IDisposable
{
    private const uint TextureWidth = 900;
    private const uint TextureHeight = 500;
    private const float OxygenHitBoundaryX = 650f;

    private readonly Overlay _overlay;
    private bool _visible;
    private bool _disposed;

    public event Action<string>? CommandRequested;

    public VrOverlayMenu()
    {
        _overlay = new Overlay(
            "gamemods.subnautica.vrcheats.menu",
            "Subnautica VR Cheats")
        {
            WidthInMeters = 1.15f,
            InputMethod = VROverlayInputMethod.Mouse,
            MouseScale = new HmdVector2_t
            {
                v0 = TextureWidth,
                v1 = TextureHeight
            }
        };

        // OpenVR's HMD-relative coordinate system looks down -Z.
        _overlay.TrackedDevice = Overlay.TrackedDeviceRole.Hmd;
        _overlay.Transform = new HmdMatrix34_t
        {
            m0 = 1, m1 = 0, m2 = 0, m3 = 0,
            m4 = 0, m5 = 1, m6 = 0, m7 = -0.02f,
            m8 = 0, m9 = 0, m10 = 1, m11 = -1.15f
        };

        UploadTexture();
        _overlay.Hide();
    }

    public void Toggle()
    {
        if (_visible)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        if (_visible)
            return;

        _overlay.Show();
        _visible = true;
    }

    public void Hide()
    {
        if (!_visible)
            return;

        _overlay.Hide();
        _visible = false;
    }

    public void PollEvents()
    {
        if (!_visible || _disposed)
            return;

        var evt = new VREvent_t();
        var size = (uint)Marshal.SizeOf<VREvent_t>();

        while (OpenVR.Overlay.PollNextOverlayEvent(_overlay.Handle, ref evt, size))
        {
            if ((EVREventType)evt.eventType != EVREventType.VREvent_MouseButtonDown)
                continue;

            var x = evt.data.mouse.x;

            if (x < OxygenHitBoundaryX)
            {
                CommandRequested?.Invoke("oxygen");
                return;
            }

            Hide();
            return;
        }
    }

    private void UploadTexture()
    {
        var pixels = MenuTexture.Create((int)TextureWidth, (int)TextureHeight);
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

        try
        {
            var error = OpenVR.Overlay.SetOverlayRaw(
                _overlay.Handle,
                handle.AddrOfPinnedObject(),
                TextureWidth,
                TextureHeight,
                4);

            if (error != EVROverlayError.None)
                throw new InvalidOperationException($"SetOverlayRaw failed: {error}");
        }
        finally
        {
            handle.Free();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            _overlay.Hide();
        }
        catch
        {
            // SteamVR may already be shutting down.
        }

        try
        {
            _overlay.Destroy();
        }
        catch
        {
            // SteamVR may already be shutting down.
        }
    }
}
