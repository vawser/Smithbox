using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Nodes;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Who may call the server, and which files a command may touch.
///     <list type="bullet">
///         <item>Requests from web pages are refused: a Host other than the loopback address (DNS rebinding), an
///         Origin header, or Sec-Fetch-Site other than "none" (a URL typed into the address bar still works).</item>
///         <item>With --remote-token or SMITHBOX_REMOTE_TOKEN set, every request needs it, as the X-Smithbox-Token
///         header or the token argument.</item>
///         <item>File arguments must be absolute paths inside the loaded project, the system temp folder or a folder
///         from --remote-roots / SMITHBOX_REMOTE_ROOTS (separated by the platform's path separator); ref= may also
///         read from the game data. Symbolic links are resolved before the check.</item>
///         <item>cfg does not set string settings to paths, and command only takes Map Editor commands (map/...).</item>
///     </list>
/// </summary>
public static partial class RemoteControl
{
    // Every argument that names a file or folder
    private static readonly string[] PathKeys = { "path", "dir", "file", "faces", "ref", "out", "before", "after" };

    private static string Token =>
        GetArg("--remote-token") is { Length: > 0 } t ? t : Environment.GetEnvironmentVariable("SMITHBOX_REMOTE_TOKEN");

    /// <summary>Null if the request may run, else why not. Takes the token out of the arguments.</summary>
    private static string CheckCaller(Dictionary<string, string> headers, JsonObject args)
    {
        if (headers.TryGetValue("Host", out var host))
        {
            var port = (Listener?.LocalEndpoint as IPEndPoint)?.Port;
            var allowed = new[] { $"127.0.0.1:{port}", $"localhost:{port}", $"[::1]:{port}" };
            if (!allowed.Contains(host.Trim(), StringComparer.OrdinalIgnoreCase))
                return $"Host '{host}' is not allowed";
        }

        if (headers.ContainsKey("Origin"))
            return "Requests from web pages are not allowed";

        if (headers.TryGetValue("Sec-Fetch-Site", out var site) && !site.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
            return "Requests from web pages are not allowed";

        var given = headers.TryGetValue("X-Smithbox-Token", out var h) ? h.Trim() : GetString(args, "token");
        args.Remove("token");

        if (Token is { Length: > 0 } token && given != token)
            return "Missing or wrong token";

        return null;
    }

    /// <summary>Starts a command after checking its file arguments</summary>
    private static IEnumerator StartChecked(string command, JsonObject args, JsonObject result)
    {
        CheckPaths(command, args);
        return Commands[command](args, result);
    }

    private static void CheckPaths(string command, JsonObject args)
    {
        foreach (var key in PathKeys)
        {
            if (GetString(args, key) is not string value)
                continue;

            // A JSON list of commands (batch cmds=) is not a path
            if (key == "path" && command == "batch" && args["cmds"] != null)
                continue;

            var full = RealPath(value) ?? throw new RemoteException($"{key}= must be an absolute path");
            var roots = AllowedRoots(key == "ref");

            if (!roots.Any(r => IsInside(full, r)))
                throw new RemoteException($"{key}= is outside the allowed folders ({string.Join(", ", roots)}); " +
                                          "add more with --remote-roots or SMITHBOX_REMOTE_ROOTS");
        }

        if (command == "cfg" && GetString(args, "value") is string v && LooksLikePath(v) && IsStringSetting(GetString(args, "name")))
            throw new RemoteException("cfg does not set string settings to paths");

        if (command == "command" && GetString(args, "cmd") is string cmd && !cmd.StartsWith("map/", StringComparison.Ordinal))
            throw new RemoteException("command only takes Map Editor commands (map/...)");
    }

    private static List<string> AllowedRoots(bool gameData)
    {
        var roots = new List<string> { Path.GetTempPath() };
        var project = GetProject()?.Descriptor;

        if (!string.IsNullOrEmpty(project?.ProjectPath))
            roots.Add(project.ProjectPath);

        if (gameData && !string.IsNullOrEmpty(project?.DataPath))
            roots.Add(project.DataPath);

        var extra = GetArg("--remote-roots") ?? Environment.GetEnvironmentVariable("SMITHBOX_REMOTE_ROOTS") ?? "";
        roots.AddRange(extra.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return roots.Select(RealPath).Where(r => r != null).Distinct().ToList();
    }

    /// <summary>The absolute path with "..", "." and symbolic links of its existing part resolved; null if not rooted</summary>
    private static string RealPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            return null;

        var full = Path.GetFullPath(path);
        var root = Path.GetPathRoot(full)!;
        var current = root;

        foreach (var part in full[root.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, part);
            FileSystemInfo info = Directory.Exists(current) ? new DirectoryInfo(current) : new FileInfo(current);

            if (info.Exists && info.LinkTarget != null && info.ResolveLinkTarget(true) is { } target)
                current = Path.GetFullPath(target.FullName);
        }

        return current;
    }

    private static bool IsInside(string path, string root)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var r = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return path.Equals(root.TrimEnd(Path.DirectorySeparatorChar), comparison) || path.StartsWith(r, comparison);
    }

    private static bool LooksLikePath(string value) =>
        value.IndexOfAny(new[] { '/', '\\', ':' }) >= 0 || value.Contains("..", StringComparison.Ordinal);

    private static bool IsStringSetting(string name)
    {
        try
        {
            return name != null && MemberType(FindConfigMember(name)) == typeof(string);
        }
        catch (RemoteException)
        {
            // Unknown setting: cfg itself reports it
            return false;
        }
    }
}
