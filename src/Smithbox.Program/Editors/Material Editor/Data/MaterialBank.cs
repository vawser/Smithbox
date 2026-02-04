using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public ConcurrentDictionary<FileDictionaryEntry, MTDWrapper> MTDs = new();
    public ConcurrentDictionary<FileDictionaryEntry, MATBINWrapper> MATBINs = new();

    private ConcurrentDictionary<string, MTD> MTDLookup = new();
    private ConcurrentDictionary<string, MATBIN> MATBINLookup = new();

    public MaterialBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        var mtdTasks = Project.Locator.MTD_Files.Entries.Select(async entry =>
        {
            var wrapper = new MTDWrapper(Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MTDs[entry] = wrapper;
                foreach (var kv in wrapper.Entries)
                {
                    var shortName = Path.GetFileNameWithoutExtension(kv.Key.Replace('\\', Path.DirectorySeparatorChar));
                    MTDLookup[shortName] = kv.Value;
                }
            }
        });

        var matbinTasks = Project.Locator.MATBIN_Files.Entries.Select(async entry =>
        {
            var wrapper = new MATBINWrapper(Project, entry, TargetFS);
            if (await wrapper.Load())
            {
                MATBINs[entry] = wrapper;
                foreach (var kv in wrapper.Entries)
                {
                    var shortName = Path.GetFileNameWithoutExtension(kv.Key.Replace('\\', Path.DirectorySeparatorChar));
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

    public async Task<bool> Save(MaterialEditorView view)
    {
        await Task.Yield();

        if(view.Selection.SourceType is MaterialSourceType.MTD)
        {
            await view.Selection.MTDWrapper.Save(view);
        }

        if (view.Selection.SourceType is MaterialSourceType.MATBIN)
        {
            await view.Selection.MATBINWrapper.Save(view);
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        MTDs.Clear();
        MATBINs.Clear();
        MTDLookup.Clear();
        MATBINLookup.Clear();

        MTDs = null;
        MATBINs = null;
        MTDLookup = null;
        MATBINLookup = null;
    }
    #endregion
}

public class MTDWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;
    public string Name { get; set; }
    public string Path { get; set; }
    public Dictionary<string, MTD> Entries { get; set; } = new();

    public MTDWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
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

            if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
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
                            TaskLogs.AddError($"[Material Editor] Failed to read {entry.Name} as MTD", e);

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
                            TaskLogs.AddError($"[Material Editor] Failed to read {entry.Name} as MTD", e);

                            return false;
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"[Material Editor] Failed to read {Path}",  e);
            return false;
        }
    }

    public async Task<bool> Save(MaterialEditorView view)
    {
        await Task.Yield();

        if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            try
            {
                var binderData = TargetFS.ReadFileOrThrow(Path);
                var binder = BND4.Read(binderData);

                foreach (var entry in binder.Files)
                {
                    if (entry.Name == view.Selection.SelectedFileKey)
                    {
                        try
                        {
                            entry.Bytes = view.Selection.SelectedMTD.Write();
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddError($"[Material Editor] Failed to write {entry.Name} as MTD", e);
                        }
                    }
                }

                var newBinderData = binder.Write();
                Project.VFS.ProjectFS.WriteFile(view.Selection.SelectedBinderEntry.Path, newBinderData);
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[Material Editor] Failed to write {Path}", e);

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
                    if (entry.Name == view.Selection.SelectedFileKey)
                    {
                        try
                        {
                            entry.Bytes = view.Selection.SelectedMTD.Write();
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddError($"[aterial Editor] Failed to write {entry.Name} as MTD", e);
                        }
                    }
                }

                var newBinderData = binder.Write();
                Project.VFS.ProjectFS.WriteFile(view.Selection.SelectedBinderEntry.Path, newBinderData);
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[Material Editor] Failed to write {Path}", e);
                return false;
            }
        }

        return true;
    }
}

public class MATBINWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;
    public string Name { get; set; }
    public string Path { get; set; }
    public Dictionary<string, MATBIN> Entries { get; set; } = new();

    public MATBINWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
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
                        TaskLogs.AddError($"[Material Editor] Failed to read {entry.Name} as MATBIN", e);
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"[Material Editor] Failed to read {Path}", e);
            return false;
        }
    }

    public async Task<bool> Save(MaterialEditorView view)
    {
        await Task.Yield();

        try
        {
            var binderData = TargetFS.ReadFileOrThrow(Path);
            var binder = BND4.Read(binderData);

            foreach (var entry in binder.Files)
            {
                if(entry.Name == view.Selection.SelectedFileKey)
                {
                    try
                    {
                        entry.Bytes = view.Selection.SelectedMATBIN.Write();
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddError($"[Material Editor] Failed to write {entry.Name} as MATBIN", e);
                    }
                }
            }

            var newBinderData = binder.Write();
            Project.VFS.ProjectFS.WriteFile(view.Selection.SelectedBinderEntry.Path, newBinderData);
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"[Material Editor] Failed to write {Path}", e);
            return false;
        }

        return true;
    }
}