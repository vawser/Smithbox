using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Application;

/// <summary>
/// Reading and writing of the user's configuration files (Configuration.json, Interface.json, the bindings), so a
/// file that can't be read or written doesn't stop Smithbox from starting or closing, and doesn't lose the user's
/// settings.
/// </summary>
public static class ConfigFiles
{
    // Files that couldn't be read and couldn't be backed up either. Saving over them would lose what's in them, so
    // they're left alone for the rest of the session.
    private static readonly HashSet<string> ProtectedFiles = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object Lock = new();

    /// <summary>
    /// Writes the file, through a temporary file so that a crash or a full disk during the write can't leave it
    /// half-written. Throws on failure. A file protected by <see cref="KeepUnreadable"/> is skipped.
    /// </summary>
    public static void Write(string file, string content)
    {
        lock (Lock)
        {
            if (ProtectedFiles.Contains(file))
                return;
        }

        var dir = Path.GetDirectoryName(file);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var temp = file + ".tmp";
        try
        {
            File.WriteAllText(temp, content);
            File.Move(temp, file, true);
        }
        catch
        {
            try { File.Delete(temp); } catch { }
            throw;
        }
    }

    /// <summary>
    /// <see cref="Write"/>, logging a warning instead of throwing. For saves made while Smithbox runs, starts or
    /// closes, which mustn't crash it.
    /// </summary>
    public static bool TryWrite(string file, string content, Type owner)
    {
        try
        {
            Write(file, content);
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogWarning(owner, LOC.Get("SYS_Config_Save_Failed", file), e);
            return false;
        }
    }

    /// <summary>
    /// Called when a configuration file couldn't be read, before the defaults replace it. Keeps a copy of it as
    /// <c>.bak</c>; if that can't be made either (the file may be locked), the file is protected from being saved over
    /// for the rest of the session.
    /// </summary>
    public static void KeepUnreadable(string file, Type owner)
    {
        var backup = file + ".bak";

        try
        {
            File.Copy(file, backup, true);
            Smithbox.LogWarning(owner, LOC.Get("SYS_Config_Backed_Up", backup));
        }
        catch (Exception e)
        {
            lock (Lock)
            {
                ProtectedFiles.Add(file);
            }

            Smithbox.LogWarning(owner, LOC.Get("SYS_Config_Backup_Failed", file), e);
        }
    }
}
