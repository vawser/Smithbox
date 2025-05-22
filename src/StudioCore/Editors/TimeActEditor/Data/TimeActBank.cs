using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static StudioCore.Configuration.Settings.TimeActEditorTab;

namespace StudioCore.Editors.TimeActEditor.Bank;

public class TimeActBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, BinderContents> Entries = new();

    public TimeActBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> taeTask = SetupTimeActs();
        bool taeTaskResult = await taeTask;

        return true;
    }

    public async Task<bool> SetupTimeActs()
    {
        await Task.Yield();

        Entries = new();

        foreach (var entry in Project.TimeActData.TimeActFiles.Entries)
        {
            Entries.Add(entry, null);
        }

        return true;
    }

    public async Task<bool> LoadTimeActBinder(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        if (fileEntry.Extension == "anibnd")
        {
            // If already loaded, just ignore
            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                return true;
            }

            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var binderEntry = Entries.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (binderEntry.Key != null)
                {
                    var key = binderEntry.Key;

                    try
                    {
                        var taeBinderData = TargetFS.ReadFileOrThrow(key.Path);

                        if (Project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                        {
                            try
                            {
                                var taeBinder = BND3.Read(taeBinderData);

                                var files = new Dictionary<BinderFile, TAE>();

                                foreach (var file in taeBinder.Files)
                                {
                                    if (!file.Name.EndsWith("tae"))
                                        continue;

                                    var data = file.Bytes;

                                    // Some TAE files are empty, ignore them
                                    if (data.Length == 0)
                                        continue;

                                    try
                                    {
                                        var taeData = TAE.Read(data);

                                        files.Add(file, taeData);
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {file.Name} as TAE.", LogLevel.Error, Tasks.LogPriority.High, e);
                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = taeBinder;
                                newBinderContents.Files = files;

                                Entries[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {key.Filename} as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                                return false;
                            }
                        }
                        else
                        {
                            try
                            {
                                var taeBinder = BND4.Read(taeBinderData);

                                var files = new Dictionary<BinderFile, TAE>();

                                foreach (var file in taeBinder.Files)
                                {
                                    if (!file.Name.EndsWith("tae"))
                                        continue;

                                    var data = file.Bytes;

                                    // Some TAE files are empty, ignore them
                                    if (data.Length == 0)
                                        continue;

                                    try
                                    {
                                        var taeData = TAE.Read(data);

                                        files.Add(file, taeData);
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {file.Name} as TAE.", LogLevel.Error, Tasks.LogPriority.High, e);
                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = taeBinder;
                                newBinderContents.Files = files;

                                Entries[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {key.Filename} as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                                return false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {key.Filename} from VFS.", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        // Loose tae files
        else if (fileEntry.Extension == "tae")
        {
            // If already loaded, just ignore
            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                return true;
            }

            // Basically creates a fake binder to store the loose ESD in so it fits the standard system.
            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var scriptEntry = Entries.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (scriptEntry.Key != null)
                {
                    var key = scriptEntry.Key;

                    if (Project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                    {
                        var fakeBinder = new BND3();

                        try
                        {
                            var taeFileData = TargetFS.ReadFileOrThrow(key.Path);

                            // Create binder file
                            var binderFile = new BinderFile();
                            binderFile.ID = 0;
                            binderFile.Name = fileEntry.Filename;
                            binderFile.Bytes = taeFileData;
                            fakeBinder.Files.Add(binderFile);

                            // Load actual tae file
                            var files = new Dictionary<BinderFile, TAE>();
                            var data = binderFile.Bytes;

                            // Some TAE files are empty, ignore them
                            if (data.Length != 0)
                            {
                                try
                                {
                                    var taeData = TAE.Read(data);
                                    files.Add(binderFile, taeData);

                                    var newBinderContents = new BinderContents();
                                    newBinderContents.Name = fileEntry.Filename;
                                    newBinderContents.Binder = fakeBinder;
                                    newBinderContents.Files = files;
                                    newBinderContents.Loose = true;

                                    Entries[key] = newBinderContents;
                                }
                                catch (Exception e)
                                {
                                    TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {fileEntry.Filename} as TAE.", LogLevel.Error, Tasks.LogPriority.High, e);
                                    return false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {key.Filename} from VFS.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    else
                    {
                        var fakeBinder = new BND4();

                        try
                        {
                            var taeFileData = TargetFS.ReadFileOrThrow(key.Path);

                            // Create binder file
                            var binderFile = new BinderFile();
                            binderFile.ID = 0;
                            binderFile.Name = fileEntry.Filename;
                            binderFile.Bytes = taeFileData;
                            fakeBinder.Files.Add(binderFile);

                            // Load actual ESD file
                            var files = new Dictionary<BinderFile, TAE>();
                            var data = binderFile.Bytes;

                            // Some TAE files are empty, ignore them
                            if (data.Length != 0)
                            {
                                try
                                {
                                    var taeData = TAE.Read(data);
                                    files.Add(binderFile, taeData);

                                    var newBinderContents = new BinderContents();
                                    newBinderContents.Name = fileEntry.Filename;
                                    newBinderContents.Binder = fakeBinder;
                                    newBinderContents.Files = files;
                                    newBinderContents.Loose = true;

                                    Entries[key] = newBinderContents;
                                }
                                catch (Exception e)
                                {
                                    TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {fileEntry.Filename} as TAE.", LogLevel.Error, Tasks.LogPriority.High, e);
                                    return false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to {key.Filename} from VFS.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> SaveAllTimeActs()
    {
        await Task.Yield();

        foreach (var entry in Entries)
        {
            await SaveTimeAct(entry.Key, entry.Value);
        }

        return true;
    }

    public async Task<bool> SaveTimeAct(FileDictionaryEntry fileEntry, BinderContents curContents)
    {
        await Task.Yield();

        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
        {
            if (curContents.Loose)
            {
                // Should only ever be one file in a 'fake' binder
                var looseFile = curContents.Files.First().Value;
                if (looseFile != null)
                {
                    try
                    {
                        var taeData = looseFile.Write();

                        try
                        {
                            Project.ProjectFS.WriteFile(fileEntry.Path, taeData);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to write {fileEntry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to write {fileEntry.Filename} as TAE", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
            else
            {
                foreach (var file in curContents.Files)
                {
                    var binderFile = file.Key;
                    var taeFile = file.Value;

                    try
                    {
                        binderFile.Bytes = taeFile.Write();
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to write {binderFile.Name} as TAE", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }

                if (Project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                {
                    try
                    {
                        var binder = (BND3)curContents.Binder;
                        var binderData = binder.Write();

                        try
                        {
                            Project.ProjectFS.WriteFile(fileEntry.Path, binderData);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to write {fileEntry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to write {fileEntry.Filename} as BND4.", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        var binder = (BND4)curContents.Binder;
                        var binderData = binder.Write();

                        try
                        {
                            Project.ProjectFS.WriteFile(fileEntry.Path, binderData);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to write {fileEntry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Failed to write {fileEntry.Filename} as BND4.", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
        }

        TaskLogs.AddLog($"[{Project.ProjectName}:Time Act Editor] Saved {fileEntry.Path}.");

        return true;
    }
}


public class BinderContents
{
    public string Name { get; set; }
    public IBinder Binder { get; set; }
    public Dictionary<BinderFile, TAE> Files { get; set; }

    /// <summary>
    /// This is to mark a 'fake' binder used for the loose tAE files
    /// </summary>
    public bool Loose { get; set; } = false;
}