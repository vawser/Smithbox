/// <summary>
///     Entry point via DOTNET_STARTUP_HOOKS: runs before Smithbox's Main. Without --remote-port or SMITHBOX_REMOTE_PORT
///     Smithbox stays unchanged.
/// </summary>
internal class StartupHook
{
    public static void Initialize()
    {
        StudioCore.Utilities.Remote.RemoteHooks.Install();
    }
}
