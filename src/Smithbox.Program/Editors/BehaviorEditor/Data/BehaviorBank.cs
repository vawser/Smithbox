using StudioCore.Core;
using StudioCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Formats.JSON;
using Microsoft.Extensions.Logging;
using System.IO;
using StudioCore.FileBrowserNS;

namespace BehaviorEditorNS;

public class BehaviorBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, BinderContents> Binders = new();

    public BehaviorBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }
    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> behaviorTask = SetupBehavior();
        bool behaviorTaskResult = await behaviorTask;

        return true;
    }
    public async Task<bool> SetupBehavior()
    {
        await Task.Yield();

        Binders = new();

        foreach (var entry in Project.BehaviorData.BehaviorFiles.Entries)
        {
            Binders.Add(entry, null);
        }

        return true;
    }

    public async Task<bool> LoadBinder(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // If already loaded, just ignore
        if (Binders.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
        {
            return true;
        }

        if (Binders.Any(e => e.Key.Filename == fileEntry.Filename))
        {
            var scriptEntry = Binders.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

            if (scriptEntry.Key != null)
            {
                var key = scriptEntry.Key;

                try
                {
                    var binderData = Project.FS.ReadFileOrThrow(key.Path);
                    var curBinder = BND4.Read(binderData);

                    var newBinderContents = new BinderContents();
                    newBinderContents.Name = key.Filename;

                    var fileList = new List<BinderFile>();
                    foreach (var file in curBinder.Files)
                    {
                        fileList.Add(file);
                    }

                    newBinderContents.Binder = curBinder;
                    newBinderContents.Files = fileList;

                    if (Binders.ContainsKey(key))
                    {
                        Binders[key] = newBinderContents;
                    }
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor:{Name}] Failed to load {key.Path}", LogLevel.Error, StudioCore.Tasks.LogPriority.High, ex);
                }
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public async Task<bool> SaveBinder(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        if (Binders.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
        {
            var targetBinder = Binders[fileEntry];

            try
            {
                var binderData = targetBinder.Binder.Write();
                Project.ProjectFS.WriteFile(fileEntry.Path, binderData);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to write {fileEntry.Filename}", LogLevel.Error, StudioCore.Tasks.LogPriority.High, e);
                return false;
            }
        }

        return true;
    }
}

public class BinderContents
{
    public string Name { get; set; }
    public BND4 Binder { get; set; }
    public List<BinderFile> Files { get; set; }
}