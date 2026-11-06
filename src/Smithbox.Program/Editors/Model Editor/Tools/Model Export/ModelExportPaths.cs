using System;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Checks the paths the Model Export tool reads and writes, which come from directories typed into its settings, so
/// a bad setting is reported by name instead of failing partway through with an exception.
/// </summary>
public static class ModelExportPaths
{
    public static string OutputDirectoryName => LOC.Get("MODEL_ModelExport_Output_Directory_Name");

    /// <summary>
    /// Builds the path of a file in a directory setting, and checks it as <see cref="TryValidateFilePath"/> does.
    /// An empty setting is an error rather than the program's own folder, which is what combining with it would give.
    /// </summary>
    /// <param name="directoryName">The setting's name, for the error</param>
    public static bool TryResolveFilePath(string directory, string fileName, string directoryName,
        out string fullPath, out string error)
    {
        fullPath = null;

        if (string.IsNullOrWhiteSpace(directory))
        {
            error = LOC.Get("MODEL_ModelExport_Error_Directory_Not_Set", directoryName);
            return false;
        }

        if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(fileName)))
        {
            error = LOC.Get("MODEL_ModelExport_Error_No_Source_FLVER");
            return false;
        }

        return TryValidateFilePath(Path.Combine(directory, fileName), directoryName, out fullPath, out error);
    }

    /// <summary>
    /// Checks that a file path is well formed, with a file name, and that its folder exists. The folder isn't
    /// created, as a mistyped directory would otherwise make folders in unexpected places. Whether the file itself
    /// exists is left to the caller, as only some callers read it.
    /// </summary>
    /// <param name="directoryName">The setting the folder came from, for the error</param>
    /// <param name="fullPath">The path made absolute, when it is valid</param>
    /// <param name="error">Why the path can't be used, when it isn't valid</param>
    public static bool TryValidateFilePath(string path, string directoryName, out string fullPath, out string error)
    {
        fullPath = null;
        error = null;

        if (string.IsNullOrWhiteSpace(path))
        {
            error = LOC.Get("MODEL_ModelExport_Error_Directory_Not_Set", directoryName);
            return false;
        }

        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception e) when (e is ArgumentException or NotSupportedException or PathTooLongException)
        {
            fullPath = null;
            error = LOC.Get("MODEL_ModelExport_Error_Invalid_Directory_Path", directoryName, path);
            return false;
        }

        var fileName = Path.GetFileName(fullPath);
        var directory = Path.GetDirectoryName(fullPath);

        if (directory == null || string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(fileName)))
        {
            fullPath = null;
            error = LOC.Get("MODEL_ModelExport_Error_Invalid_Directory_Path", directoryName, path);
            return false;
        }

        // GetFullPath accepts characters Windows won't, such as the quotes of a path copied from Explorer, or a colon
        // past the drive (C:\a:b, an NTFS stream). Each folder and the file name are checked as names on their own.
        var root = Path.GetPathRoot(fullPath) ?? "";
        var invalidNameChars = Path.GetInvalidFileNameChars();
        var names = fullPath[root.Length..].Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);

        if (names.Any(name => name.IndexOfAny(invalidNameChars) >= 0))
        {
            fullPath = null;
            error = LOC.Get("MODEL_ModelExport_Error_Invalid_Directory_Path", directoryName, path);
            return false;
        }

        if (!Directory.Exists(directory))
        {
            fullPath = null;
            error = LOC.Get("MODEL_ModelExport_Error_Missing_Directory", directoryName, directory);
            return false;
        }

        return true;
    }
}
