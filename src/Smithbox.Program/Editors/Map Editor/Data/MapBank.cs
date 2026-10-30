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
        
        foreach(var entry in Project.Locator.MapFiles.Entries)
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

        var adapter = MsbGameAdapter.For(Project.Descriptor.ProjectType);

        // No adapter means this game's maps cannot be read yet
        if (adapter == null)
            return false;

        try
        {
            if (!TargetFS.FileExists(Path))
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_VPS", Path));
                return false;
            }

            var mapData = TargetFS.ReadFile(Path);

            try
            {
                MSB = adapter.Read(mapData.Value);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_MSB", Path), e);
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_VPS", Path), e);
            return false;
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        MSB = null;
        MapContainer = null;
    }
    #endregion
}