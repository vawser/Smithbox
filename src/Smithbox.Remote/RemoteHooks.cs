using System;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Attaches the remote control to Smithbox without changing it: RemoteControl.Tick runs on Smithbox.FrameUpdated
///     (the server starts on the first frame, the port is shown in the window title). Screenshots use the viewport's
///     own snapshot (VulkanViewport.Snapshot).
/// </summary>
public static class RemoteHooks
{
    private static bool Started;

    public static void Install()
    {
        if (HasPort())
            Smithbox.FrameUpdated += OnFrame;
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
}
