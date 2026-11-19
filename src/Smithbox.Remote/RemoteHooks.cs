using HarmonyLib;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Numerics;
using Veldrid;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Attaches the remote control to Smithbox at run time without changing its sources:
///     RemoteControl.Tick runs on Smithbox.FrameUpdated (started on the first frame, port shown in the window title),
///     and a requested screenshot is recorded after ToneMapPass.Draw of the map viewport (Harmony postfix).
/// </summary>
public static class RemoteHooks
{
    private static bool Started;

    public static void Install()
    {
        if (!HasPort())
            return;

        Smithbox.FrameUpdated += OnFrame;

        var harmony = new Harmony("smithbox.remote");
        var toneMap = AccessTools.Method(typeof(ToneMapPass), nameof(ToneMapPass.Draw),
            [typeof(GraphicsDevice), typeof(CommandList), typeof(Texture), typeof(OutputDescription), typeof(Vector2),
             typeof(GparamLighting), typeof(AutoExposurePass), typeof(bool)])
            ?? throw new MissingMethodException("ToneMapPass.Draw not found: the remote control does not match this Smithbox version");
        harmony.Patch(toneMap, postfix: new HarmonyMethod(typeof(RemoteHooks), nameof(ToneMapPostfix)));
    }

    private static bool HasPort()
    {
        var args = Environment.GetCommandLineArgs();
        return Array.IndexOf(args, "--remote-port") >= 0
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SMITHBOX_REMOTE_PORT"));
    }

    private static void OnFrame()
    {
        if (!Started)
        {
            Started = true;
            RemoteControl.Start();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => RemoteControl.Stop();
        }

        RemoteControl.Tick();

        // Smithbox sets the title again when a project is loaded
        var window = Smithbox.Instance?._context?.Window;
        var suffix = RemoteControl.TitleSuffix;
        if (window != null && suffix.Length > 0 && !window.Title.EndsWith(suffix))
            window.Title += suffix;
    }

    private static void ToneMapPostfix(ToneMapPass __instance, GraphicsDevice gd, CommandList cl, Texture source,
        GparamLighting lighting, AutoExposurePass autoExposure, bool passThrough)
    {
        if (!ViewportCapture.TryGetOwner(__instance, out var viewport, out var capture)
            || capture.State != ViewportCapture.CaptureState.Requested)
            return;

        var target = viewport.ViewPipeline.Target;
        capture.Record(gd, cl, source, target.Width, target.Height, lighting, autoExposure, passThrough);

        // Record binds its own target; Smithbox goes on drawing to the swapchain afterwards
        cl.SetFramebuffer(viewport.Device.SwapchainFramebuffer);
        cl.SetViewport(0, viewport.RenderViewport);
    }
}
