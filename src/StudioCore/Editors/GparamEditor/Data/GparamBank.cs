using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, GPARAM> Entries = new();

    public GparamBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        Task<bool> gparamTask = SetupGraphicsParams();
        bool gparamTaskResult = await gparamTask;

        return true;
    }

    public async Task<bool> SetupGraphicsParams()
    {
        await Task.Delay(1);

        Entries = new();

        foreach (var entry in Project.GparamData.GparamFiles.Entries)
        {
            Entries.Add(entry, null);
        }

        return true;
    }

    public async Task<bool> LoadGraphicsParam(FileDictionaryEntry fileEntry)
    {
        await Task.Delay(1);

        // If already loaded, just ignore
        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
        {
            return true;
        }

        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename))
        {
            var scriptEntry = Entries.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

            if (scriptEntry.Key != null)
            {
                var key = scriptEntry.Key;

                var gparamData = TargetFS.ReadFileOrThrow(key.Path);
                var gparam = GPARAM.Read(gparamData);

                Entries[key] = gparam;
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public async Task<bool> SaveAllGraphicsParams()
    {
        await Task.Delay(1);

        foreach (var entry in Entries)
        {
            await SaveGraphicsParam(entry.Key, entry.Value);
        }

        return true;
    }

    public async Task<bool> SaveGraphicsParam(FileDictionaryEntry fileEntry, GPARAM gparamEntry)
    {
        await Task.Delay(1);

        var bytes = gparamEntry.Write();

        Project.ProjectFS.WriteFile(fileEntry.Path, bytes);

        return true;
    }
}
