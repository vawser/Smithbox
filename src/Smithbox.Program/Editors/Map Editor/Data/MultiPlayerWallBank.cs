using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Holds the MPW files of Elden Ring open world tiles, which place the walls that keep players in a tile
/// during multiplayer. Each small tile of m60 and m61 has one, with the same name as its MSB. A file is
/// read when its map is loaded, and its panels become entities of the map.
/// </summary>
public class MultiPlayerWallBank
{
    public ProjectEntry Project;

    // The compression of each file read, so it is written back the same way
    private readonly Dictionary<string, DCX.Type> Compression = new(StringComparer.OrdinalIgnoreCase);

    public MultiPlayerWallBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.ER)
            return true;

        return false;
    }

    private FileDictionaryEntry GetEntry(string mapId)
    {
        return Project.Locator.MultiPlayerWallFiles.Entries.FirstOrDefault(e => e.Filename == mapId);
    }

    public void LoadMPW(MapContainer map)
    {
        if (!CanUse())
            return;

        var entry = GetEntry(map.Name);

        if (entry == null)
            return;

        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        try
        {
            if (!fs.FileExists(entry.Path))
                return;

            var fileData = fs.ReadFile(entry.Path);
            var data = DCX.Decompress(fileData.Value, out var compression);

            var mpw = MPW.Read(data);

            lock (Compression)
            {
                Compression[map.Name] = compression;
            }

            map.LoadMPW(map.Name, mpw);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_MPW", entry.Path), e);
        }
    }

    public bool SaveMPW(MapContainer map)
    {
        if (!CanUse() || map.MultiPlayerWallParent == null)
            return true;

        var entry = GetEntry(map.Name);

        if (entry == null)
            return true;

        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        try
        {
            var mpw = new MPW();

            foreach (var child in map.MultiPlayerWallParent.Children)
            {
                if (child.WrappedObject is MPW.Panel panel)
                {
                    mpw.Panels.Add(panel);
                }
            }

            DCX.Type compression;

            lock (Compression)
            {
                compression = Compression.GetValueOrDefault(map.Name, DCX.Type.DCX_KRAK);
            }

            var newBytes = mpw.Write();

            // Only write the file if the panels have changed, so an unedited tile isn't added to the project
            if (fs.FileExists(entry.Path))
            {
                var oldBytes = DCX.Decompress(fs.ReadFile(entry.Path).Value, out _);

                if (BytePerfectHelper.Md5Equal(oldBytes.Span, newBytes))
                    return true;
            }

            Project.VFS.ProjectFS.WriteFile(entry.Path, DCX.Compress(newBytes, compression));

            Smithbox.Log(this, LOC.Get("MAP_Data_Save_MPW_PASS", map.Name));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_MPW", entry.Path), e);
            return false;
        }
    }
}
