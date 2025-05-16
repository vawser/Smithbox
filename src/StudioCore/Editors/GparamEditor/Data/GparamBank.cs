using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, GPARAM> Entries = new();

    public GparamBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
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

        foreach (var entry in Project.GparamData.GparamFiles.Entries)
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
                        TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to read {key.Path} as GPARAM", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to read {key.Path} from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
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
                Project.ProjectFS.WriteFile(fileEntry.Path, bytes);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to write {fileEntry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to write {fileEntry.Filename} as GPARAM", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }

        return true;
    }
}
