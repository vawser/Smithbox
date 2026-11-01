using Andre.IO.VFS;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;
    public string Name;

    public Dictionary<FileDictionaryEntry, TimeActContainerWrapper> Models = new();

    public Dictionary<FileDictionaryEntry, TimeActContainerWrapper> Characters = new();
    public Dictionary<FileDictionaryEntry, TimeActContainerWrapper> Assets = new();
    public Dictionary<FileDictionaryEntry, TimeActContainerWrapper> Parts = new();

    public TimeActBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Characters
        foreach (var entry in Project.Locator.ChrFiles.Entries)
        {
            var newEntry = new TimeActContainerWrapper(Project, entry, TargetFS);

            if (!Characters.ContainsKey(entry))
                Characters.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Assets
        foreach (var entry in Project.Locator.AssetFiles.Entries)
        {
            var newEntry = new TimeActContainerWrapper(Project, entry, TargetFS);

            if (!Assets.ContainsKey(entry))
                Assets.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Parts
        foreach (var entry in Project.Locator.PartFiles.Entries)
        {
            var newEntry = new TimeActContainerWrapper(Project, entry, TargetFS);

            if (!Parts.ContainsKey(entry))
                Parts.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Models.Clear();
        Characters.Clear();
        Assets.Clear();
        Parts.Clear();

        Models = null;
        Characters = null;
        Assets = null;
        Parts = null;
    }
    #endregion
}
