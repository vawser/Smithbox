using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.EzStateEditorNS;

public class EsdBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, BinderContents> Scripts = new();

    public EsdBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> esdTask = SetupESD();
        bool esdTaskResult = await esdTask;

        return true;
    }

    public async Task<bool> SetupESD()
    {
        await Task.Yield();

        Scripts = new();

        foreach (var entry in Project.EsdData.EsdFiles.Entries)
        {
            Scripts.Add(entry, null);
        }

        return true;
    }

    /// <summary>
    /// For talkesdbnd
    /// </summary>
    /// <param name="fileEntry"></param>
    /// <returns></returns>
    public async Task<bool> LoadScriptBinder(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // Standard talk binders
        if (fileEntry.Extension == "talkesdbnd")
        {
            // If already loaded, just ignore
            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                return true;
            }

            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var scriptEntry = Scripts.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (scriptEntry.Key != null)
                {
                    var key = scriptEntry.Key;

                    try
                    {
                        var esdBinderData = TargetFS.ReadFileOrThrow(key.Path);

                        if (Project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                        {
                            try
                            {
                                var esdBinder = BND3.Read(esdBinderData);

                                var files = new Dictionary<BinderFile, ESD>();

                                foreach (var file in esdBinder.Files)
                                {
                                    var data = file.Bytes;

                                    try
                                    {
                                        var esdData = ESD.Read(data);

                                        files.Add(file, esdData);
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {file.Name} as ESD.", LogLevel.Error, Tasks.LogPriority.High, e);
                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = esdBinder;
                                newBinderContents.Files = files;

                                Scripts[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {key.Filename} as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                                return false;
                            }
                        }
                        else
                        {
                            try
                            {
                                var esdBinder = BND4.Read(esdBinderData);

                                var files = new Dictionary<BinderFile, ESD>();

                                foreach (var file in esdBinder.Files)
                                {
                                    var data = file.Bytes;

                                    try
                                    {
                                        var esdData = ESD.Read(data);

                                        files.Add(file, esdData);
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {file.Name} as ESD.", LogLevel.Error, Tasks.LogPriority.High, e);
                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = esdBinder;
                                newBinderContents.Files = files;

                                Scripts[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {key.Filename} as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                                return false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {key.Filename} from VFS.", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        // Loose esd files
        else if(fileEntry.Extension == "esd")
        {
            // If already loaded, just ignore
            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                return true;
            }

            // Basically creates a fake binder to store the loose ESD in so it fits the standard system.
            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var scriptEntry = Scripts.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (scriptEntry.Key != null)
                {
                    var key = scriptEntry.Key;

                    if (Project.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                    {
                        var fakeBinder = new BND3();

                        try
                        {
                            var esdFileData = TargetFS.ReadFileOrThrow(key.Path);

                            // Create binder file
                            var binderFile = new BinderFile();
                            binderFile.ID = 0;
                            binderFile.Name = fileEntry.Filename;
                            binderFile.Bytes = esdFileData;
                            fakeBinder.Files.Add(binderFile);

                            // Load actual ESD file
                            var files = new Dictionary<BinderFile, ESD>();
                            var data = binderFile.Bytes;

                            try
                            {
                                var esdData = ESD.Read(data);
                                files.Add(binderFile, esdData);

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = fakeBinder;
                                newBinderContents.Files = files;
                                newBinderContents.Loose = true;

                                Scripts[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {fileEntry.Filename} as ESD.", LogLevel.Error, Tasks.LogPriority.High, e);
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {key.Filename} from VFS.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    else
                    {
                        var fakeBinder = new BND4();

                        try
                        {
                            var esdFileData = TargetFS.ReadFileOrThrow(key.Path);

                            // Create binder file
                            var binderFile = new BinderFile();
                            binderFile.ID = 0;
                            binderFile.Name = fileEntry.Filename;
                            binderFile.Bytes = esdFileData;
                            fakeBinder.Files.Add(binderFile);

                            // Load actual ESD file
                            var files = new Dictionary<BinderFile, ESD>();
                            var data = binderFile.Bytes;

                            try
                            {
                                var esdData = ESD.Read(data);
                                files.Add(binderFile, esdData);

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = fakeBinder;
                                newBinderContents.Files = files;
                                newBinderContents.Loose = true;

                                Scripts[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {fileEntry.Filename} as ESD.", LogLevel.Error, Tasks.LogPriority.High, e);
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to {key.Filename} from VFS.", LogLevel.Error, Tasks.LogPriority.High, e);
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

    public async Task<bool> SaveAllScripts()
    {
        await Task.Yield();

        foreach (var entry in Scripts)
        {
            await SaveScript(entry.Key, entry.Value);
        }

        return true;
    }

    public async Task<bool> SaveScript(FileDictionaryEntry fileEntry, BinderContents curContents)
    {
        await Task.Yield();

        if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
        {
            if(curContents.Loose)
            {
                // Should only ever be one file in a 'fake' binder
                var looseFile = curContents.Files.First().Value;
                if(looseFile != null)
                {
                    try
                    {
                        var esdData = looseFile.Write();

                        try
                        {
                            Project.ProjectFS.WriteFile(fileEntry.Path, esdData);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write {fileEntry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write {fileEntry.Filename} as ESD", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
            else
            {
                foreach(var file in curContents.Files)
                {
                    var binderFile = file.Key;
                    var esdFile = file.Value;

                    try
                    {
                        binderFile.Bytes = esdFile.Write();
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write {binderFile.Name} as ESD", LogLevel.Error, Tasks.LogPriority.High, e);
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
                            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write {fileEntry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to write {fileEntry.Filename} as BND4.", LogLevel.Error, Tasks.LogPriority.High, e);
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
                            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to write {fileEntry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to write {fileEntry.Filename} as BND4.", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
        }

        return true;
    }
}

public class BinderContents
{
    public string Name { get; set; }
    public IBinder Binder { get; set; }
    public Dictionary<BinderFile, ESD> Files { get; set; }

    /// <summary>
    /// This is to mark a 'fake' binder used for the loose ESD files
    /// </summary>
    public bool Loose { get; set; } = false;
}