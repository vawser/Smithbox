using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, GPARAM> Entries = new();

    public GparamBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> gparamTask = SetupGraphicsParams();
        bool gparamTaskResult = await gparamTask;

        return true;
    }

    public async Task<bool> SetupGraphicsParams()
    {
        await Task.Yield();

        Entries = new();

        foreach (var entry in Project.Locator.GparamFiles.Entries)
        {
            Entries.Add(entry, null);
        }

        return true;
    }

    public async Task<bool> LoadGraphicsParam(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // If already loaded, just ignore
        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
        {
            return true;
        }

        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename))
        {
            var scriptEntry = Entries.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

            if (scriptEntry.Key != null)
            {
                var key = scriptEntry.Key;

                try
                {
                    var gparamData = TargetFS.ReadFileOrThrow(key.Path);

                    try
                    {
                        var gparam = GPARAM.Read(gparamData);

                        Entries[key] = gparam;
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Graphics Param Editor] Failed to read {key.Path} as GPARAM for {Name}.", e);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Graphics Param Editor] Failed to read {key.Path} from VFS for {Name}.", e);
                    return false;
                }
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public async Task<bool> SaveAllGraphicsParams()
    {
        await Task.Yield();

        foreach (var entry in Entries)
        {
            await SaveGraphicsParam(entry.Key, entry.Value);
        }

        return true;
    }

    public async Task<bool> SaveGraphicsParam(FileDictionaryEntry fileEntry, GPARAM gparamEntry)
    {
        await Task.Yield();

        try
        {
            var bytes = gparamEntry.Write();

            try
            {
                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, bytes);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Graphics Param Editor] Failed to write {fileEntry.Filename} as file for {Name}.", e);
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Graphics Param Editor] Failed to write {fileEntry.Filename} as GPARAM for {Name}.", e);
            return false;
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Entries.Clear();

        Entries = null;
    }
    #endregion
}
