using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Data;

public class MapBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public FileDictionary MapFiles = new();

    public Dictionary<string, MapWrapper> Maps = new();

    public MapBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);
        
        MapFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "msb").ToList();

        foreach(var entry in MapFiles.Entries)
        {
            var newMapEntry = new MapWrapper(BaseEditor, Project, entry, TargetFS);
            Maps.Add(entry.Filename, newMapEntry);
        }

        return true;
    }

    public async Task<bool> LoadMap(string mapID, bool msbOnly = false)
    {
        if(Maps.ContainsKey(mapID))
        {
            await Maps[mapID].Load(msbOnly);
        }
        else
        {
            return false;
        }

        return true;
    }
    public async Task<bool> SaveMap(string mapID, bool seralizeContainer = true)
    {
        if (Maps.ContainsKey(mapID))
        {
            // Seralize container back to MSB here
            if (seralizeContainer)
            {

            }

            await Maps[mapID].Save();
        }
        else
        {
            return false;
        }

        return true;
    }
}

public class MapWrapper
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    public string Name { get; set; }
    public string Path { get; set; }

    /// <summary>
    /// This is the 'truth' for the map data
    /// </summary>
    public IMsb MSB { get; set; }

    /// <summary>
    /// This is deseralized from the MSB, and seralized back on save
    /// </summary>
    public MapContainer MapContainer { get; set; }

    public MapWrapper(Smithbox baseEditor, ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
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
        var mapData = TargetFS.ReadFileOrThrow(Path);

        switch (Project.ProjectType)
        {
            case ProjectType.DES:
                MSB = MSBD.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                MSB = MSB1.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                MSB = MSB2.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.DS3:
                MSB = MSB3.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.BB:
                MSB = MSBB.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.SDT:
                MSB = MSBS.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.ER:
                MSB = MSBE.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.AC6:
                MSB = MSB_AC6.Read(mapData);
                successfulLoad = true;
                break;
            case ProjectType.ERN:
            default: break;
        }

        // Map Container setup
        if (!msbOnly)
        {
            MapContainer = new(editor, Name);
            MapContainer.LoadMSB(MSB);
        }

        return successfulLoad;
    }

    public async Task<bool> Save()
    {
        await Task.Delay(1);

        var successfulSave = false;

        switch (Project.ProjectType)
        {
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
            case ProjectType.DS3:
            case ProjectType.BB:
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.AC6:
            case ProjectType.ERN:
            default: break;
        }

        return successfulSave;
    }
}