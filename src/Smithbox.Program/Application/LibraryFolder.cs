using System;
using System.IO;
using System.Runtime.InteropServices;

namespace StudioCore.Application;

/// <summary>
/// The release build keeps the executables at the top of the install folder and moves the
/// supporting binaries (runtime, NuGet packages, native libraries) into a Libs folder.
/// Smithbox.ReleaseLayout writes a localPath into each deps.json, so the .NET host already
/// loads managed assemblies and DllImport targets from there; this covers the rest.
/// </summary>
public static class LibraryFolder
{
    public const string FolderName = "Libs";

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectoryW(string pathName);

    /// <summary>
    /// Call first in Main, before anything loads a native library.
    /// Does nothing for a flat layout (development builds, Linux and macOS releases).
    /// </summary>
    public static void Setup()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var baseDir = AppContext.BaseDirectory;
        var libsDir = Path.Join(baseDir, FolderName);

        if (!Directory.Exists(libsDir))
            return;

        RemoveSupersededRootFiles(baseDir, libsDir);

        // Libraries that load native code with a plain LoadLibrary("x.dll"), rather than through
        // the .NET host's probing, search the executable's folder and then this one.
        SetDllDirectoryW(libsDir);
    }

    /// <summary>
    /// The updater extracts a release over the existing install, so an install that predates the
    /// Libs folder keeps its old copies at the top level. Loaders that probe the executable's
    /// folder first would pick those up instead of the matching copies in Libs.
    /// The release build never puts a file that must stay at the top level into Libs.
    /// </summary>
    private static void RemoveSupersededRootFiles(string baseDir, string libsDir)
    {
        foreach (var libPath in Directory.EnumerateFiles(libsDir))
        {
            var rootPath = Path.Join(baseDir, Path.GetFileName(libPath));

            if (!File.Exists(rootPath))
                continue;

            try
            {
                File.Delete(rootPath);
            }
            catch (Exception e)
            {
                // Held open by another running Smithbox instance: it is retried on the next start
                Console.WriteLine($"Could not remove superseded file \"{rootPath}\": {e.Message}");
            }
        }
    }
}
