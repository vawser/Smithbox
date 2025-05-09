using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public FileDictionary MTD_Files = new();
    public FileDictionary MATBIN_Files = new();

    public Dictionary<string, MTDWrapper> MTDs = new();
    public Dictionary<string, MATBINWrapper> MATBINs = new();

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

        MTD_Files.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "mtdbnd").ToList();

        foreach (var entry in MTD_Files.Entries)
        {
            var newMtdEntry = new MTDWrapper(BaseEditor, Project, entry, TargetFS);
            await newMtdEntry.Load();
            MTDs.Add(entry.Filename, newMtdEntry);
        }

        MATBIN_Files.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "matbinbnd").ToList();

        foreach (var entry in MATBIN_Files.Entries)
        {
            var newMatbinEntry = new MATBINWrapper(BaseEditor, Project, entry, TargetFS);
            await newMatbinEntry.Load();
            MATBINs.Add(entry.Filename, newMatbinEntry);
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
        var binderData = TargetFS.ReadFileOrThrow(Path);

        var binder = BND4.Read(binderData);
        foreach (var entry in binder.Files)
        {
            if (entry.Name.Contains(".mtd"))
            {
                var mtd = MTD.Read(entry.Bytes);
                Entries.Add(entry.Name, mtd);
            }
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
        var binderData = TargetFS.ReadFileOrThrow(Path);

        var binder = BND4.Read(binderData);
        foreach(var entry in binder.Files)
        {
            if(entry.Name.Contains(".matbin"))
            {
                var matbin = MATBIN.Read(entry.Bytes);
                Entries.Add(entry.Name, matbin);
            }
        }

        return successfulLoad;
    }
}