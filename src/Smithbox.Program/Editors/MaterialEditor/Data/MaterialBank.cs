using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

public class MaterialBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public ConcurrentDictionary<FileDictionaryEntry, MTDWrapper> MTDs = new();
    public ConcurrentDictionary<FileDictionaryEntry, MATBINWrapper> MATBINs = new();

    private ConcurrentDictionary<string, MTD> MTDLookup = new();
    private ConcurrentDictionary<string, MATBIN> MATBINLookup = new();

    public MaterialBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        var mtdTasks = Project.MaterialData.MTD_Files.Entries.Select(async entry =>
        {
            var wrapper = new MTDWrapper(BaseEditor, Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MTDs[entry] = wrapper;
                foreach (var kv in wrapper.Entries)
                {
                    var shortName = Path.GetFileNameWithoutExtension(kv.Key);
                    MTDLookup[shortName] = kv.Value;
                }
            }
        });

        var matbinTasks = Project.MaterialData.MATBIN_Files.Entries.Select(async entry =>
        {
            var wrapper = new MATBINWrapper(BaseEditor, Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MATBINs[entry] = wrapper;
                foreach (var kv in wrapper.Entries)
                {
                    var shortName = Path.GetFileNameWithoutExtension(kv.Key);
                    MATBINLookup[shortName] = kv.Value;
                }
            }
        });

        await Task.WhenAll(mtdTasks.Concat(matbinTasks));

        return true;
    }

    public MTD GetMaterial(string name) =>
        MTDLookup.TryGetValue(name, out var mtd) ? mtd : null;

    public MATBIN GetMatbin(string name) =>
        MATBINLookup.TryGetValue(name, out var matbin) ? matbin : null;

    public async Task<bool> Save(MaterialEditorScreen editor)
    {
        await Task.Yield();

        if(editor.Selection.SourceType is SourceType.MTD)
        {
            await editor.Selection.MTDWrapper.Save(editor);
        }

        if (editor.Selection.SourceType is SourceType.MATBIN)
        {
            await editor.Selection.MATBINWrapper.Save(editor);
        }

        return true;
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

    public async Task<bool> Load()
    {
        await Task.Yield();

        try
        {
            var binderData = TargetFS.ReadFileOrThrow(Path);

            if (Project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
            {
                var binder = BND3.Read(binderData);

                foreach (var entry in binder.Files)
                {
                    if (entry.Name.Contains(".mtd"))
                    {
                        try
                        {
                            var mtd = MTD.Read(entry.Bytes);

                            if (Entries.ContainsKey(entry.Name))
                            {
                                Entries[entry.Name] = mtd;
                            }
                            else
                            {
                                Entries.Add(entry.Name, mtd);
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {entry.Name} as MTD", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                }
            }
            else
            {
                var binder = BND4.Read(binderData);

                foreach (var entry in binder.Files)
                {
                    if (entry.Name.Contains(".mtd"))
                    {
                        try
                        {
                            var mtd = MTD.Read(entry.Bytes);

                            if(Entries.ContainsKey(entry.Name))
                            {
                                Entries[entry.Name] = mtd;
                            }
                            else
                            {
                                Entries.Add(entry.Name, mtd);
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {entry.Name} as MTD", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {Path}", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }
    }

    public async Task<bool> Save(MaterialEditorScreen editor)
    {
        await Task.Yield();

        if (Project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            try
            {
                var binderData = TargetFS.ReadFileOrThrow(Path);
                var binder = BND4.Read(binderData);

                foreach (var entry in binder.Files)
                {
                    if (entry.Name == editor.Selection.SelectedFileKey)
                    {
                        try
                        {
                            entry.Bytes = editor.Selection.SelectedMTD.Write();
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to write {entry.Name} as MTD", LogLevel.Error, Tasks.LogPriority.High, e);
                        }
                    }
                }

                var newBinderData = binder.Write();
                Project.ProjectFS.WriteFile(editor.Selection.SelectedBinderEntry.Path, newBinderData);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to write {Path}", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        else
        {
            try
            {
                var binderData = TargetFS.ReadFileOrThrow(Path);
                var binder = BND4.Read(binderData);

                foreach (var entry in binder.Files)
                {
                    if (entry.Name == editor.Selection.SelectedFileKey)
                    {
                        try
                        {
                            entry.Bytes = editor.Selection.SelectedMTD.Write();
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to write {entry.Name} as MTD", LogLevel.Error, Tasks.LogPriority.High, e);
                        }
                    }
                }

                var newBinderData = binder.Write();
                Project.ProjectFS.WriteFile(editor.Selection.SelectedBinderEntry.Path, newBinderData);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to write {Path}", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }

        return true;
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

    public async Task<bool> Load()
    {
        await Task.Yield();

        try
        {
            var binderData = TargetFS.ReadFileOrThrow(Path);
            var binder = BND4.Read(binderData);

            foreach (var entry in binder.Files)
            {
                if (entry.Name.Contains(".matbin"))
                {
                    try
                    {
                        var matbin = MATBIN.Read(entry.Bytes);
                        if (Entries.ContainsKey(entry.Name))
                        {
                            Entries[entry.Name] = matbin;
                        }
                        else
                        {
                            Entries.Add(entry.Name, matbin);
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {entry.Name} as MATBIN", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to read {Path}", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }
    }

    public async Task<bool> Save(MaterialEditorScreen editor)
    {
        await Task.Yield();

        try
        {
            var binderData = TargetFS.ReadFileOrThrow(Path);
            var binder = BND4.Read(binderData);

            foreach (var entry in binder.Files)
            {
                if(entry.Name == editor.Selection.SelectedFileKey)
                {
                    try
                    {
                        entry.Bytes = editor.Selection.SelectedMATBIN.Write();
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to write {entry.Name} as MATBIN", LogLevel.Error, Tasks.LogPriority.High, e);
                    }
                }
            }

            var newBinderData = binder.Write();
            Project.ProjectFS.WriteFile(editor.Selection.SelectedBinderEntry.Path, newBinderData);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to write {Path}", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }

        return true;
    }
}