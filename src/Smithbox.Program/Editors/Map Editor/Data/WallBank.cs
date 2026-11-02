using HKX2;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The triangles of a low collision that a WALL file can mark, in the space of its part.
/// </summary>
public sealed class WallGeometry
{
    private readonly Dictionary<uint, int> Lookup = new();

    public readonly List<uint> Keys = new();
    public readonly List<Vector3> Corners = new();
    public BoundingBox Bounds { get; private set; }

    public static WallGeometry Build(fsnpCustomParamCompressedMeshShape shape, hknpBodyCinfo body)
    {
        var geometry = new WallGeometry();
        var data = shape.m_data;
        var tree = data.m_meshTree;

        for (var s = 0; s < tree.m_sections.Count; s++)
        {
            var count = (int)(tree.m_sections[s].m_primitives.m_data & 0xFF);

            for (var p = 0; p < count; p++)
            {
                for (var half = 0; half < 2; half++)
                {
                    var key = data.GetShapeKey(s, p, half);

                    if (!data.TryGetShapeKeyTriangle(key, out var a, out var b, out var c))
                        continue;

                    geometry.Lookup[key] = geometry.Keys.Count;
                    geometry.Keys.Add(key);
                    geometry.Corners.Add(HKX2_Helper.TransformVert(a, body));
                    geometry.Corners.Add(HKX2_Helper.TransformVert(b, body));
                    geometry.Corners.Add(HKX2_Helper.TransformVert(c, body));
                }
            }
        }

        if (geometry.Corners.Count > 0)
        {
            geometry.Bounds = BoundingBox.CreateFromVertices(geometry.Corners.ToArray());
        }

        return geometry;
    }

    public bool TryGetTriangle(uint key, out Vector3 a, out Vector3 b, out Vector3 c)
    {
        if (!Lookup.TryGetValue(key, out var index))
        {
            a = b = c = default;
            return false;
        }

        a = Corners[index * 3];
        b = Corners[index * 3 + 1];
        c = Corners[index * 3 + 2];
        return true;
    }

    /// <summary>
    /// Finds the nearest triangle the ray hits, with the ray in the space of the part.
    /// </summary>
    public bool RayCast(Ray ray, out uint key, out float distance)
    {
        key = 0;
        distance = float.MaxValue;

        if (Keys.Count == 0 || !ray.Intersects(Bounds))
            return false;

        var found = false;

        for (var i = 0; i < Keys.Count; i++)
        {
            if (IntersectTriangle(ray, Corners[i * 3], Corners[i * 3 + 1], Corners[i * 3 + 2], out var t) && t < distance)
            {
                distance = t;
                key = Keys[i];
                found = true;
            }
        }

        return found;
    }

    // Moller-Trumbore, which hits either side of the triangle
    private static bool IntersectTriangle(Ray ray, Vector3 a, Vector3 b, Vector3 c, out float t)
    {
        t = 0;

        var edge1 = b - a;
        var edge2 = c - a;
        var p = Vector3.Cross(ray.Direction, edge2);
        var det = Vector3.Dot(edge1, p);

        if (MathF.Abs(det) < 1e-8f)
            return false;

        var invDet = 1 / det;
        var s = ray.Origin - a;
        var u = Vector3.Dot(s, p) * invDet;

        if (u < 0 || u > 1)
            return false;

        var q = Vector3.Cross(s, edge1);
        var v = Vector3.Dot(ray.Direction, q) * invDet;

        if (v < 0 || u + v > 1)
            return false;

        t = Vector3.Dot(edge2, q) * invDet;
        return t > 0;
    }
}

/// <summary>
/// The cells of a WALL file to display, split by what they are.
/// </summary>
public record WallCells(
    List<(Vector3 A, Vector3 B, Vector3 C)> Walls,
    List<(Vector3 A, Vector3 B, Vector3 C)> OtherCollisionWalls,
    List<(Vector3 A, Vector3 B, Vector3 C)> None);

/// <summary>
/// The triangle of a collision part that a ray hit.
/// </summary>
public record WallHit(MsbEntity Entity, uint ShapeKey, float Distance);

/// <summary>
/// Holds the WALL files of Sekiro maps, which mark the parts of each low collision that are walls.
/// A collision part uses the file with its model name (e.g. h000100) in the wallbhd of its map, and the
/// triangles it refers to come from the low collision of the same number. Nothing is read until walls are displayed.
/// </summary>
public class WallBank
{
    // Lifts the cells off the collision, so they aren't hidden by it
    private const float SurfaceOffset = 0.02f;

    // Applied per frame, as each part creates several meshes
    private const int MaxAppliedPerFrame = 32;

    private record WallResult(MsbEntity Entity, string Key, WallCells Cells);

    /// <summary>
    /// The WALL files of a map, keyed by model name, and the compression each was stored with.
    /// </summary>
    private sealed class MapWalls
    {
        public readonly Dictionary<string, WALL> Files = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, DCX.Type> Compression = new(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> Modified = new(StringComparer.OrdinalIgnoreCase);
    }

    private readonly ProjectEntry Project;

    private readonly ConcurrentDictionary<string, Task<MapWalls>> Maps = new();
    private readonly ConcurrentDictionary<string, WallGeometry> Geometry = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<WallResult> Completed = new();

    public bool Visible { get; private set; }

    public WallBank(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Picks up changes in what should be displayed and applies finished parts.
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
    /// Requests the walls of a collision part, if they aren't already what is displayed.
    /// </summary>
    public void Request(MsbEntity entity)
    {
        if (!Visible)
            return;

        // A part that has left the map still gets here when it is deselected
        if (entity.Parent == null || !EntityHelper.IsPartCollision(entity))
            return;

        var modelName = entity.GetPartModelName();

        if (modelName == entity.WallKey)
            return;

        entity.ClearWalls();

        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return;

        var mapId = entity.MapID;
        var walls = GetMapWalls(mapId);

        entity.WallKey = modelName;

        Task.Run(async () =>
        {
            var files = await walls;
            var geometry = GetGeometry(mapId, modelName);

            if (geometry == null)
                return;

            WALL wall;
            lock (files)
            {
                wall = files.Files.GetValueOrDefault(modelName);
            }

            Completed.Enqueue(new WallResult(entity, modelName, BuildCells(wall, geometry)));
        });
    }

    /// <summary>
    /// Rebuilds the walls of a part straight away, e.g. after it has been painted.
    /// </summary>
    public void Rebuild(MsbEntity entity)
    {
        var modelName = entity.GetPartModelName();
        var geometry = GetGeometry(entity.MapID, modelName);
        var files = GetLoadedMapWalls(entity.MapID);

        if (geometry == null || files == null)
            return;

        WALL wall;
        lock (files)
        {
            wall = files.Files.GetValueOrDefault(modelName);
        }

        entity.WallKey = modelName;
        entity.ApplyWalls(BuildCells(wall, geometry));
    }

    /// <summary>
    /// Rebuilds everything from the current data, so changes show when walls are turned back on.
    /// Edits that haven't been saved are kept.
    /// </summary>
    private void Refresh()
    {
        foreach (var entity in GetCollisionParts())
        {
            if (entity.SetupRenderMesh)
            {
                entity.ClearWalls();
                entity.RefreshWalls();
            }
        }
    }

    private void ApplyCompleted()
    {
        for (var i = 0; i < MaxAppliedPerFrame && Completed.TryDequeue(out var result); i++)
        {
            // The part may have changed, or left the map, since this was requested
            if (result.Entity.WallKey == result.Key && result.Entity.RenderSceneMesh != null)
            {
                result.Entity.ApplyWalls(result.Cells);
            }
        }
    }

    private IEnumerable<MsbEntity> GetCollisionParts()
    {
        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
                continue;

            foreach (var ent in entry.Value.MapContainer.Objects)
            {
                if (ent is MsbEntity msbEnt && EntityHelper.IsPartCollision(msbEnt))
                {
                    yield return msbEnt;
                }
            }
        }
    }

    #region Cells

    private static WallCells BuildCells(WALL wall, WallGeometry geometry)
    {
        var cells = new WallCells(new(), new(), new());

        if (wall == null)
            return cells;

        foreach (var triangle in wall.Triangles)
        {
            if (!geometry.TryGetTriangle(triangle.ShapeKey, out var a, out var b, out var c))
                continue;

            var normal = Vector3.Cross(b - a, c - a);
            if (normal.LengthSquared() < 1e-12f)
                continue;

            var offset = Vector3.Normalize(normal) * SurfaceOffset;

            AddCell(wall, triangle, a + offset, b + offset, c + offset, cells);
        }

        return cells;
    }

    /// <summary>
    /// Adds the unsplit cells within a cell. Points are weights of the second and third corners of the triangle.
    /// </summary>
    private static void AddCell(WALL wall, WALL.Cell cell, Vector3 a, Vector3 b, Vector3 c, WallCells cells)
    {
        Vector3 GetPoint(ushort index)
        {
            if (index == 0)
                return a;
            if (index == 1)
                return b;
            if (index == 2)
                return c;

            var point = index - 3 < wall.Points.Count ? wall.Points[index - 3] : Vector2.Zero;
            return a + (b - a) * point.X + (c - a) * point.Y;
        }

        if (cell.Children.Count > 0)
        {
            foreach (var child in cell.Children)
            {
                AddCell(wall, child, a, b, c, cells);
            }

            return;
        }

        var corners = (GetPoint(cell.Index1), GetPoint(cell.Index2), GetPoint(cell.Index3));

        switch (cell.Type)
        {
            case WALL.CellType.Wall when cell.CollisionID == wall.ID:
                cells.Walls.Add(corners);
                break;
            case WALL.CellType.Wall:
                cells.OtherCollisionWalls.Add(corners);
                break;
            case WALL.CellType.None:
                cells.None.Add(corners);
                break;
        }
    }

    #endregion

    #region Painting

    /// <summary>
    /// Finds the nearest painted or paintable triangle under the ray, out of the collision parts whose walls are displayed.
    /// </summary>
    public WallHit RayCast(Ray ray)
    {
        WallHit nearest = null;

        foreach (var entity in GetCollisionParts())
        {
            if (entity.WallKey == null || !entity.EditorVisible || entity.WallMeshes == null)
                continue;

            var geometry = GetGeometry(entity.MapID, entity.WallKey);

            if (geometry == null || !Matrix4x4.Invert(entity.GetWorldMatrix(), out var inverse))
                continue;

            var origin = Vector3.Transform(ray.Origin, inverse);
            var direction = Vector3.TransformNormal(ray.Direction, inverse);
            var scale = direction.Length();

            if (scale < 1e-8f)
                continue;

            if (!geometry.RayCast(new Ray(origin, direction / scale), out var key, out var distance))
                continue;

            // Back into world units, so parts with a different scale compare correctly
            distance /= scale;

            if (nearest == null || distance < nearest.Distance)
            {
                nearest = new WallHit(entity, key, distance);
            }
        }

        return nearest;
    }

    /// <summary>
    /// Gets the corners of a triangle of a part, in the space of the part.
    /// </summary>
    public bool TryGetTriangle(MsbEntity entity, uint key, out Vector3 a, out Vector3 b, out Vector3 c)
    {
        var geometry = GetGeometry(entity.MapID, entity.GetPartModelName());

        if (geometry != null)
            return geometry.TryGetTriangle(key, out a, out b, out c);

        a = b = c = default;
        return false;
    }

    /// <summary>
    /// Gets a copy of how a triangle is marked, or null if the WALL file doesn't have it.
    /// </summary>
    public WALL.Triangle GetTriangle(string mapId, string modelName, uint key)
    {
        var files = GetLoadedMapWalls(mapId);

        if (files == null)
            return null;

        lock (files)
        {
            var wall = files.Files.GetValueOrDefault(modelName);
            var index = wall == null ? -1 : FindTriangle(wall, key);

            return index >= 0 ? wall.Triangles[index].Clone() : null;
        }
    }

    /// <summary>
    /// Replaces how a triangle is marked with a copy of the given one, or removes it if null.
    /// Creates the WALL file if the collision doesn't have one.
    /// </summary>
    public void SetTriangle(string mapId, string modelName, uint key, WALL.Triangle triangle)
    {
        var files = GetLoadedMapWalls(mapId);

        if (files == null)
            return;

        lock (files)
        {
            if (!files.Files.TryGetValue(modelName, out var wall))
            {
                if (triangle == null || !long.TryParse(modelName.Substring(1), out var id))
                    return;

                wall = new WALL { ID = id };
                files.Files[modelName] = wall;
            }

            var index = FindTriangle(wall, key);

            if (triangle == null)
            {
                if (index >= 0)
                {
                    wall.Triangles.RemoveAt(index);
                }
            }
            else
            {
                var copy = triangle.Clone();
                copy.ShapeKey = key;

                if (index >= 0)
                {
                    wall.Triangles[index] = copy;
                }
                else
                {
                    wall.Triangles.Insert(~index, copy);
                }
            }

            files.Modified.Add(modelName);
        }
    }

    /// <summary>
    /// Creates a triangle marked entirely as one type.
    /// </summary>
    public static WALL.Triangle CreateTriangle(string modelName, uint key, WALL.CellType type)
    {
        long.TryParse(modelName.Substring(1), out var id);

        return new WALL.Triangle
        {
            ShapeKey = key,
            Index1 = 0,
            Index2 = 1,
            Index3 = 2,
            Type = type,
            CollisionID = type is WALL.CellType.Wall ? id : 0
        };
    }

    /// <summary>
    /// The triangles are kept in ascending order of shape key, as the game expects. Returns the
    /// index of the triangle, or the complement of where it would be inserted.
    /// </summary>
    private static int FindTriangle(WALL wall, uint key)
    {
        int low = 0, high = wall.Triangles.Count - 1;

        while (low <= high)
        {
            var mid = (low + high) / 2;
            var midKey = wall.Triangles[mid].ShapeKey;

            if (midKey == key)
                return mid;

            if (midKey < key)
                low = mid + 1;
            else
                high = mid - 1;
        }

        return ~low;
    }

    #endregion

    #region Files

    /// <summary>
    /// Forgets the collision of a map, which is read again when the map is loaded. WALL files that have
    /// been painted are kept until they are saved; the others are read again too.
    /// </summary>
    public void OnUnloadMap(string mapId)
    {
        var prefix = $"l{mapId.Substring(1)}_";

        foreach (var key in Geometry.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            Geometry.TryRemove(key, out _);
        }

        var walls = GetLoadedMapWalls(mapId);

        if (walls != null)
        {
            lock (walls)
            {
                if (walls.Modified.Count > 0)
                    return;
            }

            Maps.TryRemove(mapId, out _);
        }
    }

    private Task<MapWalls> GetMapWalls(string mapId)
    {
        return Maps.GetOrAdd(mapId, _ => Task.Run(() => ReadMapFiles(mapId)));
    }

    /// <summary>
    /// The WALL files of a map, if they have finished being read.
    /// </summary>
    private MapWalls GetLoadedMapWalls(string mapId)
    {
        return Maps.TryGetValue(mapId, out var task) && task.IsCompletedSuccessfully ? task.Result : null;
    }

    /// <summary>
    /// Whether the WALL files of a map have been read, so it can be painted.
    /// </summary>
    public bool IsMapLoaded(string mapId)
    {
        return GetLoadedMapWalls(mapId) != null;
    }

    private WallGeometry GetGeometry(string mapId, string modelName)
    {
        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return null;

        // e.g. l10_00_00_00_000100.hkx.dcx for h000100
        var fileName = $"l{mapId.Substring(1)}_{modelName.Substring(1)}.hkx.dcx";

        if (Geometry.TryGetValue(fileName, out var existing))
            return existing;

        var container = Project.Handler.MapData.HavokCollisionBank.GetTagContainer(fileName);

        if (container == null)
            return null;

        var scene = container.m_namedVariants
            .Select(v => v.m_variant)
            .OfType<hknpPhysicsSceneData>()
            .FirstOrDefault();

        var body = scene?.m_systemDatas
            .SelectMany(s => s.m_bodyCinfos)
            .FirstOrDefault(b => b.m_shape is fsnpCustomParamCompressedMeshShape);

        if (body == null)
            return null;

        var geometry = WallGeometry.Build((fsnpCustomParamCompressedMeshShape)body.m_shape, body);

        return Geometry.GetOrAdd(fileName, geometry);
    }

    private MapWalls ReadMapFiles(string mapId)
    {
        var walls = new MapWalls();

        var bhdPath = Path.Join("map", mapId, $"{mapId}.wallbhd");
        var bdtPath = Path.Join("map", mapId, $"{mapId}.wallbdt");

        try
        {
            if (!Project.VFS.FS.FileExists(bhdPath) || !Project.VFS.FS.FileExists(bdtPath))
                return walls;

            var bhdData = (Memory<byte>)Project.VFS.FS.ReadFile(bhdPath);
            var bdtData = (Memory<byte>)Project.VFS.FS.ReadFile(bdtPath);

            using var binder = BXF4.Read(bhdData, bdtData);

            foreach (var file in binder.Files)
            {
                // e.g. h000100.wall.dcx. The binder also holds .nojumpwall files, which are another format
                if (!file.Name.EndsWith(".wall.dcx", StringComparison.OrdinalIgnoreCase))
                    continue;

                var name = GetFileName(file.Name).Split('.')[0];
                var data = DCX.Decompress(file.Bytes, out var compression);

                walls.Files[name] = WALL.Read(data);
                walls.Compression[name] = compression;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Wall_Failed_To_Read", bhdPath), e);
        }

        return walls;
    }

    /// <summary>
    /// Writes the WALL files of a map that have been painted. Returns false if they couldn't be written.
    /// </summary>
    public bool SaveWalls(string mapId)
    {
        var walls = GetLoadedMapWalls(mapId);

        if (walls == null)
            return true;

        var bhdPath = Path.Join("map", mapId, $"{mapId}.wallbhd");
        var bdtPath = Path.Join("map", mapId, $"{mapId}.wallbdt");

        lock (walls)
        {
            if (walls.Modified.Count == 0)
                return true;

            try
            {
                BXF4 binder;

                if (Project.VFS.FS.FileExists(bhdPath) && Project.VFS.FS.FileExists(bdtPath))
                {
                    var bhdData = (Memory<byte>)Project.VFS.FS.ReadFile(bhdPath);
                    var bdtData = (Memory<byte>)Project.VFS.FS.ReadFile(bdtPath);

                    binder = BXF4.Read(bhdData, bdtData);
                }
                else
                {
                    binder = CreateBinder();
                }

                using (binder)
                {
                    foreach (var name in walls.Modified)
                    {
                        if (!walls.Files.TryGetValue(name, out var wall))
                            continue;

                        var compression = walls.Compression.GetValueOrDefault(name, DCX.Type.DCX_KRAK);
                        var bytes = DCX.Compress(wall.Write(), compression);

                        var file = binder.Files.FirstOrDefault(e =>
                            GetFileName(e.Name).Equals($"{name}.wall.dcx", StringComparison.OrdinalIgnoreCase));

                        if (file != null)
                        {
                            file.Bytes = bytes;
                        }
                        else
                        {
                            var nextId = binder.Files.Count == 0 ? 0 : binder.Files.Max(e => e.ID) + 1;
                            binder.Files.Add(new BinderFile(Binder.FileFlags.Flag1, nextId, $"{name}.wall.dcx", bytes));
                        }
                    }

                    binder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

                    Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
                    Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
                }

                walls.Modified.Clear();
                Smithbox.Log(this, LOC.Get("MAP_Wall_Save_PASS", mapId));
                return true;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Wall_Failed_To_Write", bhdPath), e);
                return false;
            }
        }
    }

    /// <summary>
    /// A binder for a map that has no WALL files yet, set up like the ones the game has.
    /// </summary>
    private static BXF4 CreateBinder()
    {
        return new BXF4
        {
            Version = "07D7R6",
            Format = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression,
            Unicode = false,
            Extended = 4
        };
    }

    private static string GetFileName(string path)
    {
        return path.Substring(path.LastIndexOf('\\') + 1);
    }

    #endregion
}
