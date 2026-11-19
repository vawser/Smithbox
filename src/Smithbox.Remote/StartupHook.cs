using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

/// <summary>
///     Entry point via DOTNET_STARTUP_HOOKS: runs before Smithbox' Main. Without --remote-port or SMITHBOX_REMOTE_PORT
///     Smithbox stays unchanged.
/// </summary>
internal class StartupHook
{
    public static void Initialize()
    {
        // 0Harmony.dll is not in Smithbox' deps.json, so it is looked up next to this DLL
        var dir = Path.GetDirectoryName(typeof(StartupHook).Assembly.Location);
        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            var path = Path.Join(dir, name.Name + ".dll");
            return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
        };

        Install();
    }

    // Separate method so Harmony is only loaded after the Resolving handler is registered
    private static void Install()
    {
        StudioCore.Utilities.Remote.RemoteHooks.Install();
    }
}
