using System;
using System.IO;

namespace StudioCore.Utilities;

/// <summary>
/// Oodle (oo2core) is proprietary and is not shipped with Smithbox.
/// Each Oodle-using game ships its own copy next to its executable, so on project load
/// any oo2core library found in the game directory is copied into the Smithbox program directory,
/// where the SoulsFormats Oodle bindings load it from.
/// </summary>
public static class OodleSetup
{
#if WINDOWS
    private static readonly string[] OodleFileNames =
    {
        "oo2core_6_win64.dll", // Sekiro, Elden Ring
        "oo2core_8_win64.dll", // Armored Core VI
        "oo2core_9_win64.dll", // Nightreign
    };
#elif OSX
    private static readonly string[] OodleFileNames =
    {
        "liboo2coremac64.2.6.dylib",
        "liboo2coremac64.2.8.dylib",
        "liboo2coremac64.2.9.dylib",
    };
#else
    private static readonly string[] OodleFileNames =
    {
        "liboo2corelinux64.so.6",
        "liboo2corelinux64.so.8",
        "liboo2corelinux64.so.9",
    };
#endif

    /// <summary>
    /// Copies every oo2core library present in <paramref name="gameDirectory"/> that the
    /// program directory does not already have. Works for any project type.
    /// </summary>
    public static void CopyFromGameDirectory(string gameDirectory)
    {
        if (string.IsNullOrEmpty(gameDirectory) || !Directory.Exists(gameDirectory))
            return;

        foreach (var fileName in OodleFileNames)
        {
            var sourcePath = Path.Join(gameDirectory, fileName);
            var targetPath = Path.Join(AppContext.BaseDirectory, fileName);

            if (File.Exists(sourcePath) && !File.Exists(targetPath))
            {
                File.Copy(sourcePath, targetPath);
            }
        }
    }
}
