using Andre.IO.VFS;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public Dictionary<FileDictionaryEntry, BehaviorContainerWrapper> Behaviors = new();

    public string Name;

    public BehaviorBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        foreach (var entry in Project.Locator.BehaviorFiles.Entries)
        {
            var newEntry = new BehaviorContainerWrapper(Project, entry, TargetFS);

            if (!Behaviors.ContainsKey(entry))
                Behaviors.Add(entry, newEntry);
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Behaviors.Clear();

        Behaviors = null;
    }
    #endregion
}
