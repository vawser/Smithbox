using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// What is displayed for an EDGE file: the positions of its grapple points, and the
/// start and end of its hangable ledges and huggable walls.
/// </summary>
public record EdgeMarkers(List<Vector3> Points, List<(Vector3 Start, Vector3 End)> Segments);

/// <summary>
/// Builds the EDGE points displayed on parts in Sekiro. These come from two places: a collision part uses
/// the file with the same ID in the edgebhd of its map, and an object uses the file in its objbnd.
/// In both cases the points are relative to the part. Nothing is read until EDGE points are displayed.
/// </summary>
public class EdgeBank
{
    // Applied per frame, as each marker of each part creates a mesh
    private const int MaxAppliedPerFrame = 128;

    private record EdgeResult(MsbEntity Entity, string Key, EdgeMarkers Markers);

    private readonly ProjectEntry Project;

    // Keyed by map ID for collisions and model name for objects
    private readonly ConcurrentDictionary<string, Task<Dictionary<string, EDGE>>> Files = new();
    private readonly ConcurrentQueue<EdgeResult> Completed = new();

    private Dictionary<string, string> ObjectPaths;

    private bool Visible = false;

    public EdgeBank(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Picks up changes in what should be displayed and applies finished parts.
    /// Checked here as the filter can be changed from several places (e.g. the presets).
    /// </summary>
    public void Update(bool visible)
    {
        visible = visible && Project.Descriptor.ProjectType is ProjectType.SDT;

        var refresh = visible && !Visible;

        Visible = visible;

        if (refresh)
        {
            Refresh();
        }

        ApplyCompleted();
    }

    /// <summary>
    /// Requests the EDGE points of a part, if they aren't already what is displayed.
    /// </summary>
    public void Request(MsbEntity entity)
    {
        if (!Visible)
            return;

        // A part that has left the map still gets here when it is deselected, and any points it
        // asked for would be treated as displayed when the part is put back
        if (entity.Parent == null)
            return;

        var isCollision = EntityHelper.IsPartCollision(entity);

        if (!isCollision && !EntityHelper.IsPartAsset(entity))
            return;

        var modelName = entity.GetPartModelName();

        if (modelName == entity.EdgeKey)
            return;

        entity.ClearEdgePoints();

        if (string.IsNullOrEmpty(modelName))
            return;

        var mapId = entity.MapID;
        var name = isCollision ? $"{mapId}_{modelName.Substring(1)}" : modelName;

        var file = isCollision
            ? Files.GetOrAdd(mapId, _ => Task.Run(() => ReadMapFiles(mapId)))
            : Files.GetOrAdd(modelName, _ => Task.Run(() => ReadObjectFile(modelName)));

        entity.EdgeKey = modelName;

        Task.Run(async () =>
        {
            var files = await file;

            if (files.TryGetValue(name, out var edge))
            {
                Completed.Enqueue(new EdgeResult(entity, modelName, GetMarkers(edge)));
            }
        });
    }

    /// <summary>
    /// Rebuilds everything from the current data, so changes to the files show when EDGE points are turned back on.
    /// </summary>
    private void Refresh()
    {
        Files.Clear();

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                foreach (var ent in entry.Value.MapContainer.Objects)
                {
                    // Entities still being set up get their points along with the rest of their drawable
                    if (ent is MsbEntity { SetupRenderMesh: true } msbEnt)
                    {
                        msbEnt.ClearEdgePoints();
                        msbEnt.RefreshEdgePoints();
                    }
                }
            }
        }
    }

    private void ApplyCompleted()
    {
        for (var i = 0; i < MaxAppliedPerFrame && Completed.TryDequeue(out var result); i++)
        {
            // The part may have changed, or left the map, since this was requested
            if (result.Entity.EdgeKey == result.Key && result.Entity.RenderSceneMesh != null)
            {
                result.Entity.ApplyEdgePoints(result.Markers);
            }
        }
    }

    /// <summary>
    /// Some edges are repeated within a file, so each marker is only returned once.
    /// </summary>
    private static EdgeMarkers GetMarkers(EDGE edge)
    {
        HashSet<Vector3> points = new();
        HashSet<(Vector3 Start, Vector3 End)> segments = new();

        foreach (var entry in edge.Edges)
        {
            if (entry.Type is EDGE.EdgeType.Grapple)
            {
                points.Add(entry.V1);
                points.Add(entry.V2);

                // The point the player is pulled towards, which is left empty on some grapple points
                if (entry.V3 != Vector3.Zero)
                {
                    points.Add(entry.V3);
                }
            }
            // A segment with no length has no direction to draw it in
            else if (entry.V1 == entry.V2)
            {
                points.Add(entry.V1);
            }
            else
            {
                segments.Add((entry.V1, entry.V2));
            }
        }

        return new EdgeMarkers(points.ToList(), segments.ToList());
    }

    private Dictionary<string, EDGE> ReadMapFiles(string mapId)
    {
        Dictionary<string, EDGE> files = new(StringComparer.OrdinalIgnoreCase);

        var bhdPath = Path.Join("map", mapId, $"{mapId}.edgebhd");
        var bdtPath = Path.Join("map", mapId, $"{mapId}.edgebdt");

        try
        {
            if (!Project.VFS.FS.FileExists(bhdPath) || !Project.VFS.FS.FileExists(bdtPath))
                return files;

            var bhdData = (Memory<byte>)Project.VFS.FS.ReadFile(bhdPath);
            var bdtData = (Memory<byte>)Project.VFS.FS.ReadFile(bdtPath);

            using var binder = BXF4.Read(bhdData, bdtData);

            foreach (var file in binder.Files)
            {
                // e.g. m10_00_00_00_000500.edge.dcx
                var name = file.Name.Substring(file.Name.LastIndexOf('\\') + 1).Split('.')[0];

                files[name] = EDGE.Read(file.Bytes);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Edge_Failed_To_Read", bhdPath), e);
        }

        return files;
    }

    private Dictionary<string, EDGE> ReadObjectFile(string modelName)
    {
        Dictionary<string, EDGE> files = new(StringComparer.OrdinalIgnoreCase);

        var path = GetObjectPath(modelName);

        if (path == null)
            return files;

        try
        {
            var data = Project.VFS.FS.ReadFile(path);

            if (data == null)
                return files;

            var binder = BND4.Read(data.Value);
            var file = binder.Files.FirstOrDefault(e => e.Name.EndsWith(".edge", StringComparison.OrdinalIgnoreCase));

            if (file != null)
            {
                files[modelName] = EDGE.Read(file.Bytes);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Edge_Failed_To_Read", path), e);
        }

        return files;
    }

    /// <summary>
    /// Gets the path of the binder that holds an object model.
    /// </summary>
    private string GetObjectPath(string modelName)
    {
        lock (Files)
        {
            if (ObjectPaths == null)
            {
                ObjectPaths = new(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in Project.Locator.AssetFiles.Entries)
                {
                    ObjectPaths.TryAdd(entry.Filename, entry.Path);
                }
            }

            return ObjectPaths.GetValueOrDefault(modelName);
        }
    }
}
