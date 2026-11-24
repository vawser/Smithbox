using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Utilities;

/// <summary>
/// Writes the crash log for the crash handlers in the Program.cs of each entry point.
/// </summary>
public static class CrashLogFile
{
    /// <summary>
    /// Writes the crash log to the "Crash Logs" folder beside the executable or, if that folder can't be written
    /// (e.g. Smithbox is under Program Files), to %LocalAppData%\Smithbox\Crash Logs. Returns the file written, or
    /// null if neither could be written.
    /// </summary>
    public static string Write(List<string> lines)
    {
        var name = $"Log {DateTime.Now:yyyy-M-dd--HH-mm-ss}.txt";

        string[] folders =
        {
            // The executable's folder, rather than the working directory, which can still be the launching folder
            Path.Join(AppContext.BaseDirectory, "Crash Logs"),
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smithbox", "Crash Logs")
        };

        foreach (var folder in folders)
        {
            try
            {
                Directory.CreateDirectory(folder);
                var path = Path.Join(folder, name);
                File.WriteAllLines(path, lines);
                return path;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't write the crash log to \"{folder}\" : {e.Message}");
            }
        }

        return null;
    }
}
