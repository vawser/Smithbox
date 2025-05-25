using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Formats.JSON;
using System;
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

    public Dictionary<FileDictionaryEntry, MapWrapper> Maps = new();

    public MapBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();
        
        foreach(var entry in Project.MapData.MapFiles.Entries)
        {
            var newMapEntry = new MapWrapper(BaseEditor, Project, entry, TargetFS);
            Maps.Add(entry, newMapEntry);
        }

        return true;
    }

    public async Task<bool> LoadMap(string mapID)
    {
        if(Maps.Any(e => e.Key.Filename == mapID))
        {
            var mapEntry = Maps.FirstOrDefault(e => e.Key.Filename == mapID);

            if (mapEntry.Value != null)
            {
                var key = mapEntry.Key;
                await Maps[key].Load();
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    // Save still handled in Universe for now

    //public async Task<bool> SaveMap(string mapID, bool seralizeContainer = true)
    //{
    //    if (Maps.Any(e => e.Key.Filename == mapID))
    //    {
    //        var fileDictEntry = Maps.FirstOrDefault(e => e.Key.Filename == mapID);

    //        if (fileDictEntry.Value != null)
    //        {
    //            var key = fileDictEntry.Key;
    //            await Maps[key].Save();
    //        }
    //    }
    //    else
    //    {
    //        return false;
    //    }

    //    return true;
    //}
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

    // TODO:
    // Include BTL
    // Include HKX (read only)
    // Include NVA (read only)


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

    public async Task<bool> Load()
    {
        await Task.Yield();

        var successfulLoad = false;

        var editor = Project.MapEditor;

        try
        {
            var mapData = TargetFS.ReadFileOrThrow(Path);

            switch (Project.ProjectType)
            {
                case ProjectType.DES:
                    try
                    {
                        MSB = MSBD.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    try
                    {
                        MSB = MSB1.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.DS2:
                case ProjectType.DS2S:
                    try
                    {
                        MSB = MSB2.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.DS3:
                    try
                    {
                        MSB = MSB3.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.BB:
                    try
                    {
                        MSB = MSBB.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.SDT:
                    try
                    {
                        MSB = MSBS.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.ER:
                    try
                    {
                        MSB = MSBE.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.AC6:
                    try
                    {
                        MSB = MSB_AC6.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                    break;
                case ProjectType.ERN:
                    //try
                    //{
                    //    MSB = MSBE.Read(mapData);
                    //    successfulLoad = true;
                    //}
                    //catch (Exception e)
                    //{
                    //    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} as MSB", LogLevel.Error, Tasks.LogPriority.High, e);
                    //    return false;
                    //}
                    break;
                default: break;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {Path} from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }

        return successfulLoad;
    }

    // Save still handled in Universe for now

    //public async Task<bool> Save()
    //{
    //    await Task.Yield();

    //    var successfulSave = false;

    //    switch (Project.ProjectType)
    //    {
    //        case ProjectType.DES:
    //        case ProjectType.DS1:
    //        case ProjectType.DS1R:
    //        case ProjectType.DS2:
    //        case ProjectType.DS2S:
    //        case ProjectType.DS3:
    //        case ProjectType.BB:
    //        case ProjectType.SDT:
    //        case ProjectType.ER:
    //        case ProjectType.AC6:
    //        case ProjectType.ERN:
    //        default: break;
    //    }

    //    return successfulSave;
    //}
}