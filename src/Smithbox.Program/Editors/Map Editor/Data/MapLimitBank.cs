using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Holds the MSLD files of Armored Core VI maps, which hold the walls around the areas of a mission. Each map has
/// one in /map/msld, with the same name as its MSB. A file is read when its map is loaded, and its areas and their
/// edges become entities of the map.
/// </summary>
public class MapLimitBank
{
    public ProjectEntry Project;

    // The unknown hash and compression of each file read, so they are written back the same way
    private readonly Dictionary<string, (uint Hash, DCX.Type Compression)> FileInfo = new(StringComparer.OrdinalIgnoreCase);

    public MapLimitBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6)
            return true;

        return false;
    }

    private FileDictionaryEntry GetEntry(string mapId)
    {
        return Project.Locator.MapLimitFiles.Entries.FirstOrDefault(e => e.Filename == mapId);
    }

    public void LoadMSLD(MapContainer map)
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

            var msld = MSLD.Read(data);

            lock (FileInfo)
            {
                FileInfo[map.Name] = (msld.Hash, compression);
            }

            map.LoadMSLD(map.Name, msld);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_MSLD", entry.Path), e);
        }
    }

    public bool SaveMSLD(MapContainer map)
    {
        if (!CanUse() || map.MapLimitParent == null)
            return true;

        var entry = GetEntry(map.Name);

        if (entry == null)
            return true;

        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        try
        {
            (uint Hash, DCX.Type Compression) info;

            lock (FileInfo)
            {
                info = FileInfo.GetValueOrDefault(map.Name, (0u, DCX.Type.DCX_KRAK_MAX));
            }

            var msld = new MSLD();
            msld.Hash = info.Hash;

            foreach (var areaEntity in map.MapLimitParent.Children)
            {
                if (areaEntity.WrappedObject is not MSLD.Area area)
                    continue;

                // A copy, so the bounds can grow to the edges without changing the entity outside of an action
                var newArea = new MSLD.Area(area);
                newArea.Edges.Clear();
                newArea.Edges.AddRange(areaEntity.Children
                    .Select(e => e.WrappedObject)
                    .OfType<MSLD.Edge>());
                newArea.ExpandBoundsToEdges();

                msld.Areas.Add(newArea);
            }

            var newBytes = msld.Write();

            // Only write the file if the areas have changed, so an unedited map isn't added to the project
            if (fs.FileExists(entry.Path))
            {
                var oldBytes = DCX.Decompress(fs.ReadFile(entry.Path).Value, out _);

                if (BytePerfectHelper.Md5Equal(oldBytes.Span, newBytes))
                    return true;
            }

            Project.VFS.ProjectFS.WriteFile(entry.Path, DCX.Compress(newBytes, info.Compression));

            Smithbox.Log(this, LOC.Get("MAP_Data_Save_MSLD_PASS", map.Name));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_MSLD", entry.Path), e);
            return false;
        }
    }
}
