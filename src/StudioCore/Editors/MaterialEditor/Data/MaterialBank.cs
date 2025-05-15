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

namespace StudioCore.MaterialEditorNS;

public class MaterialBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, MTDWrapper> MTDs = new();
    public Dictionary<FileDictionaryEntry, MATBINWrapper> MATBINs = new();

    public MaterialBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        foreach (var entry in Project.MaterialData.MTD_Files.Entries)
        {
            var newMtdEntry = new MTDWrapper(BaseEditor, Project, entry, TargetFS);
            await newMtdEntry.Load();
            MTDs.Add(entry, newMtdEntry);
        }

        foreach (var entry in Project.MaterialData.MATBIN_Files.Entries)
        {
            var newMatbinEntry = new MATBINWrapper(BaseEditor, Project, entry, TargetFS);
            await newMatbinEntry.Load();
            MATBINs.Add(entry, newMatbinEntry);
        }

        return true;
    }

    public MTD GetMaterial(string name)
    {
        MTD temp = null;

        foreach (var entry in MTDs)
        {
            var targetEntry = entry.Value.Entries.FirstOrDefault(e => Path.GetFileNameWithoutExtension(e.Key) == name);
            if(targetEntry.Value != null)
            {
                temp = targetEntry.Value;
            }
        }

        return temp;
    }
    public MATBIN GetMatbin(string name)
    {
        MATBIN temp = null;

        foreach (var entry in MATBINs)
        {
            var targetEntry = entry.Value.Entries.FirstOrDefault(e => Path.GetFileNameWithoutExtension(e.Key) == name);
            if (targetEntry.Value != null)
            {
                temp = targetEntry.Value;
            }
        }

        return temp;
    }
}

public class MTDWrapper
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;
    public string Name { get; set; }
    public string Path { get; set; }
    public Dictionary<string, MTD> Entries { get; set; } = new();

    public MTDWrapper(Smithbox baseEditor, ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        BaseEditor = baseEditor;
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        Path = dictEntry.Path;
    }

    public async Task<bool> Load(bool msbOnly = false)
    {
        await Task.Delay(1);

        var successfulLoad = false;

        var editor = Project.MapEditor;
        try
        {
            var binderData = TargetFS.ReadFileOrThrow(Path);

            try
            {
                var binder = BND4.Read(binderData);
                foreach (var entry in binder.Files)
                {
                    if (entry.Name.Contains(".mtd"))
                    {
                        try
                        {
                            var mtd = MTD.Read(entry.Bytes);
                            Entries.Add(entry.Name, mtd);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {entry.Name} as MTD", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {Path} as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {Path} from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }

        return successfulLoad;
    }
}

public class MATBINWrapper
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;
    public string Name { get; set; }
    public string Path { get; set; }
    public Dictionary<string, MATBIN> Entries { get; set; } = new();

    public MATBINWrapper(Smithbox baseEditor, ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        BaseEditor = baseEditor;
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        Path = dictEntry.Path;
    }

    public async Task<bool> Load(bool msbOnly = false)
    {
        await Task.Delay(1);

        var successfulLoad = false;

        var editor = Project.MapEditor;

        try
        {
            var binderData = TargetFS.ReadFileOrThrow(Path);

            try
            {
                var binder = BND4.Read(binderData);
                foreach (var entry in binder.Files)
                {
                    if (entry.Name.Contains(".matbin"))
                    {
                        try
                        {
                            var matbin = MATBIN.Read(entry.Bytes);
                            Entries.Add(entry.Name, matbin);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {entry.Name} as MATBIN", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {Path} as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {Path} from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }

        return successfulLoad;
    }
}