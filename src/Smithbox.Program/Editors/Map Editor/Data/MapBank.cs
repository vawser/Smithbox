using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, MapWrapper> Maps = new();

    public MapBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();
        
        foreach(var entry in Project.Handler.MapData.MapFiles.Entries)
        {
            var newMapEntry = new MapWrapper(Project, entry, TargetFS);
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

    #region Dispose
    public void Dispose()
    {
        foreach(var entry in Maps)
        {
            entry.Value.Dispose();
        }

        Maps = null;
    }
    #endregion
}

public class MapWrapper : IDisposable
{
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

    public MapWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        Path = dictEntry.Path;
    }

    public async Task<bool> Load()
    {
        await Task.Yield();

        var successfulLoad = false;

        var editor = Project.Handler.MapEditor;

        try
        {
            var mapData = TargetFS.ReadFileOrThrow(Path);

            switch (Project.Descriptor.ProjectType)
            {
                case ProjectType.DES:
                    try
                    {
                        MSB = MSBD.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);
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
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);
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
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);
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
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);
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
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);
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
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);
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
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);
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
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB",  e);
                        return false;
                    }
                    break;
                case ProjectType.NR:
                    try
                    {
                        MSB = MSB_NR.Read(mapData);
                        successfulLoad = true;
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddError($"[Map Editor] Failed to read {Path} as MSB", e);

                        return false;
                    }
                    break;
                default: break;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"[Map Editor] Failed to read {Path} from VFS", e);
            return false;
        }

        return successfulLoad;
    }

    #region Dispose
    public void Dispose()
    {
        MSB = null;
        MapContainer = null;
    }
    #endregion
}