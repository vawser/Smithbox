using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NGP navigation data of the loaded DS2 and DS2S maps (Navigation tool, NGP mode, Phases G0 and G1): each map's
/// .ngp, read the first time the map is asked for, joined to the MSB's navmesh parts and map offset as an
/// <see cref="NgpMap"/>. Edits swap in a new document (<see cref="SetNgp"/>); the map's save writes it, only if edited,
/// in the version it was read in.
/// </summary>
public class NgpBank
{
    private readonly ProjectEntry Project;

    private sealed class MapDocument
    {
        public NGP Ngp;
        public bool Edited;
    }

    private readonly Dictionary<string, MapDocument> Maps = new();

    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, NgpMap> VanillaMaps = new();

    public NgpBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse() => Profile != null;

    public NgpGameProfile Profile => NgpGameProfile.For(Project.Descriptor.ProjectType.ToString());

    private Andre.IO.VFS.VirtualFileSystem FS => Project.VFS.FS;

    public bool HasFile(MapContainer map) => CanUse() && map != null && (Maps.ContainsKey(map.Name) || FS.FileExists(Profile.NgpPath(map.Name)));

    private MapDocument GetDocument(string mapId)
    {
        if (Maps.TryGetValue(mapId, out var doc))
            return doc;

        var path = Profile.NgpPath(mapId);
        if (!FS.FileExists(path))
            return null;

        try
        {
            var data = FS.ReadFile(path).Value.ToArray();
            if (DCX.Is(data))
                data = DCX.Decompress(data, out DCX.Type _).ToArray();

            doc = new MapDocument { Ngp = NGP.Read(data) };
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Read", path), e);
            return null;
        }

        Maps[mapId] = doc;
        return doc;
    }

    /// <summary>
    /// The map's NGP as edited, joined to its MSB navmesh parts as loaded (in map order) and placed by the map offset the
    /// Map Editor holds. Null if the map has no NGP. Built anew on each call, so it reflects every edit.
    /// </summary>
    public NgpMap GetMap(MapContainer map)
    {
        if (!CanUse() || map == null)
            return null;

        var doc = GetDocument(map.Name);
        if (doc == null)
            return null;

        var parts = new List<NgpReader.PartInfo>();
        foreach (var entity in map.Objects)
        {
            if (entity.WrappedObject is MSB2.Part.Navmesh p)
                parts.Add(new NgpReader.PartInfo(p.Name, p.ModelName, entity));
        }

        // The game adds the map offset to every position; the Map Editor holds it on the map offset node
        var world = map.MapOffsetNode?.GetWorldMatrix() ?? Matrix4x4.Identity;

        return NgpReader.Build(map.Name, Profile, doc.Ngp, parts, world);
    }

    /// <summary>The map's NGP data as the game ships it, for the vanilla baseline. Safe to call from a background task.</summary>
    public NgpMap GetVanillaMap(string mapId)
    {
        return VanillaMaps.GetOrAdd(mapId, id =>
        {
            try
            {
                return NgpReader.Read(Project.VFS.VanillaFS, id, Profile);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Read", Profile.NgpPath(id)), e);
                return null;
            }
        });
    }

    /// <summary>The issues of the vanilla data of a map, which the project's are compared with.</summary>
    public List<NavIssue> GetVanillaIssues(string mapId)
    {
        var vanilla = GetVanillaMap(mapId);
        return vanilla == null ? new List<NavIssue>() : NgpValidator.Validate(vanilla);
    }

    /// <summary>Puts an NGP in place of the map's, for an edit or its undo.</summary>
    public void SetNgp(MapContainer map, NGP ngp)
    {
        var doc = GetDocument(map.Name);
        if (doc == null || ngp == null || ReferenceEquals(doc.Ngp, ngp))
            return;

        doc.Ngp = ngp;
        doc.Edited = true;
        map.MarkDirty(MapFileType.Navmesh);
    }

    /// <summary>Writes the map's NGP if it was edited.</summary>
    public bool SaveMap(MapContainer map)
    {
        if (!CanUse() || !Maps.TryGetValue(map.Name, out var doc) || !doc.Edited)
            return true;

        var path = Profile.NgpPath(map.Name);

        try
        {
            var data = doc.Ngp.Write();

            if (!FS.FileExists(path) || !BytePerfectHelper.Md5Equal(FS.ReadFile(path).Value.ToArray(), data))
                Project.VFS.ProjectFS.WriteFile(path, data);

            doc.Edited = false;
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Write", path), e);
            return false;
        }
    }

    /// <summary>Releases an unloaded map's document, unless it has unsaved edits.</summary>
    public void OnUnloadMap(string mapId)
    {
        if (Maps.TryGetValue(mapId, out var doc) && !doc.Edited)
            Maps.Remove(mapId);
    }
}
