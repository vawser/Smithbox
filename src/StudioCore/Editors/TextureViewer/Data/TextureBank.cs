using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Data;

public class TextureBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, BinderContents> Entries = new();

    public TextureBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }
    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> taeTask = SetupTextures();
        bool taeTaskResult = await taeTask;

        return true;
    }
    public async Task<bool> SetupTextures()
    {
        await Task.Yield();

        Entries = new();

        foreach (var entry in Project.TimeActData.TimeActFiles.Entries)
        {
            Entries.Add(entry, null);
        }

        return true;
    }
}

public class BinderContents
{
    public string Name { get; set; }
    public IBinder Binder { get; set; }
    public Dictionary<BinderFile, TPF> Files { get; set; }

    /// <summary>
    /// This is to mark a 'fake' binder used for the loose TPF files
    /// </summary>
    public bool Loose { get; set; } = false;
}