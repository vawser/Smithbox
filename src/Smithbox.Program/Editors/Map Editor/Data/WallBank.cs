using Havok.Shared;
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

    /// <summary>
    /// The triangles of a compressed mesh, in the world, by shape key (the WALL files refer to triangles by them).
    /// </summary>
    public static WallGeometry Build(IHavokCompressedMesh shape, IHavokBody body)
    {
        var geometry = new WallGeometry();
        var transform = CollisionGeometry.BodyTransform(body);

        foreach (var (key, a, b, c) in CollisionGeometry.CompressedTriangles(shape))
        {
            geometry.Lookup[key] = geometry.Keys.Count;
            geometry.Keys.Add(key);
            geometry.Corners.Add(Vector3.Transform(a, transform));
            geometry.Corners.Add(Vector3.Transform(b, transform));
            geometry.Corners.Add(Vector3.Transform(c, transform));
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
    List<(Vector3 A, Vector3 B, Vector3 C)> None,
    List<(Vector3 A, Vector3 B, Vector3 C)> NoJump);

/// <summary>
/// The triangle of a collision part that a ray hit.
/// </summary>
public record WallHit(MsbEntity Entity, uint ShapeKey, float Distance);

/// <summary>
/// Holds the WALL and NOJUMPWALL files of Sekiro maps. WALL files mark the parts of each low collision that are
/// walls, and NOJUMPWALL files list its triangles that can't be jumped off. A collision part uses the files with its
/// model name (e.g. h000100) in the wallbhd of its map, and the triangles they refer to come from the low collision
/// of the same number. Nothing is read until walls are displayed.
/// </summary>
public class WallBank
{
    // Lifts the cells off the collision, so they aren't hidden by it
    private const float SurfaceOffset = 0.02f;

    // No-jump triangles are lifted further, so they show over the wall cells of the same triangle
    private const float NoJumpOffset = 0.035f;

    private const string WallExtension = ".wall.dcx";
    private const string NoJumpExtension = ".nojumpwall.dcx";

    // Applied per frame, as each part creates several meshes
    private const int MaxAppliedPerFrame = 32;

    private record WallResult(MsbEntity Entity, string Key, WallCells Cells);

    /// <summary>
    /// The WALL and NOJUMPWALL files of a map, keyed by model name. Compression and changes are tracked by
    /// file name in the binder (e.g. h000100.wall.dcx), as a model can have one of each.
    /// </summary>
    private sealed class MapWalls
    {
        public readonly Dictionary<string, WALL> Files = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, NOJUMPWALL> NoJumpFiles = new(StringComparer.OrdinalIgnoreCase);
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

            WallCells cells;
            lock (files)
            {
                cells = BuildCells(files, modelName, geometry);
            }

            Completed.Enqueue(new WallResult(entity, modelName, cells));
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

        WallCells cells;
        lock (files)
        {
            cells = BuildCells(files, modelName, geometry);
        }

        entity.WallKey = modelName;
        entity.ApplyWalls(cells);
    }

    /// <summary>
    /// Rebuilds the walls of every part that displays them, e.g. after what is displayed has changed.
    /// </summary>
    public void RebuildAll()
    {
        if (!Visible)
            return;

        foreach (var entity in GetCollisionParts())
        {
            if (entity.WallKey != null && entity.WallMeshes != null)
            {
                Rebuild(entity);
            }
        }
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

    /// <summary>
    /// Builds the cells of a model's WALL file and the triangles of its NOJUMPWALL file. Called with the files locked.
    /// </summary>
    private static WallCells BuildCells(MapWalls files, string modelName, WallGeometry geometry)
    {
        var cells = new WallCells(new(), new(), new(), new());

        if (files.Files.TryGetValue(modelName, out var wall))
        {
            foreach (var triangle in wall.Triangles)
            {
                if (!geometry.TryGetTriangle(triangle.ShapeKey, out var a, out var b, out var c))
                    continue;

                var normal = GetFrontNormal(a, b, c);
                if (normal == Vector3.Zero)
                    continue;

                var offset = normal * SurfaceOffset;

                AddCell(wall, triangle, a + offset, b + offset, c + offset, cells);
            }
        }

        if (files.NoJumpFiles.TryGetValue(modelName, out var noJump))
        {
            foreach (var key in noJump.ShapeKeys)
            {
                if (!geometry.TryGetTriangle(key, out var a, out var b, out var c))
                    continue;

                var offset = GetFrontNormal(a, b, c) * NoJumpOffset;

                cells.NoJump.Add((a + offset, b + offset, c + offset));
            }
        }

        return cells;
    }

    /// <summary>
    /// The side of a collision triangle that faces out, which cells are lifted towards so they show
    /// in front of the collision. Zero if the triangle has no area.
    /// </summary>
    public static Vector3 GetFrontNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        var normal = Vector3.Cross(b - a, c - a);

        return normal.LengthSquared() < 1e-12f ? Vector3.Zero : Vector3.Normalize(normal);
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

            files.Modified.Add(modelName + WallExtension);
        }
    }

    /// <summary>
    /// Whether a triangle is listed in the NOJUMPWALL file of a collision, so it can't be jumped off.
    /// </summary>
    public bool IsNoJump(string mapId, string modelName, uint key)
    {
        var files = GetLoadedMapWalls(mapId);

        if (files == null)
            return false;

        lock (files)
        {
            return files.NoJumpFiles.TryGetValue(modelName, out var noJump) && noJump.ShapeKeys.BinarySearch(key) >= 0;
        }
    }

    /// <summary>
    /// Adds a triangle to, or removes it from, the NOJUMPWALL file of a collision, keeping the keys in ascending
    /// order. Creates the file if the collision doesn't have one.
    /// </summary>
    public void SetNoJump(string mapId, string modelName, uint key, bool noJump)
    {
        var files = GetLoadedMapWalls(mapId);

        if (files == null)
            return;

        lock (files)
        {
            if (!files.NoJumpFiles.TryGetValue(modelName, out var file))
            {
                if (!noJump)
                    return;

                file = new NOJUMPWALL();
                files.NoJumpFiles[modelName] = file;
            }

            var index = file.ShapeKeys.BinarySearch(key);

            if (noJump && index < 0)
            {
                file.ShapeKeys.Insert(~index, key);
            }
            else if (!noJump && index >= 0)
            {
                file.ShapeKeys.RemoveAt(index);
            }
            else
            {
                return;
            }

            files.Modified.Add(modelName + NoJumpExtension);
        }
    }

    /// <summary>
    /// The WALL and NOJUMPWALL marks of one collision, copied, for moving them to new shape keys and back.
    /// </summary>
    public sealed record WallMarks(List<WALL.Triangle> Triangles, List<uint> NoJumpKeys);

    /// <summary>
    /// Reads the WALL files of a map if they haven't been, waiting for them: they are otherwise only read once walls
    /// are displayed. False if they can't be read.
    /// </summary>
    public bool EnsureMapLoaded(string mapId)
    {
        try
        {
            GetMapWalls(mapId).Wait();
        }
        catch (Exception)
        {
            return false;
        }

        return IsMapLoaded(mapId);
    }

    /// <summary>A copy of the marks of a collision. Null if the map's WALL files haven't been read.</summary>
    public WallMarks GetMarks(string mapId, string modelName)
    {
        var files = GetLoadedMapWalls(mapId);

        if (files == null)
            return null;

        lock (files)
        {
            var triangles = files.Files.TryGetValue(modelName, out var wall)
                ? wall.Triangles.Select(t => t.Clone()).ToList()
                : new List<WALL.Triangle>();

            var noJump = files.NoJumpFiles.TryGetValue(modelName, out var file)
                ? new List<uint>(file.ShapeKeys)
                : new List<uint>();

            return new WallMarks(triangles, noJump);
        }
    }

    /// <summary>
    /// The marks moved to new shape keys, for after a collision is rebuilt. Marks whose key has no new key (a triangle
    /// that was left out) are dropped. The keys are kept in ascending order, as the game expects.
    /// </summary>
    public static WallMarks RemapMarks(WallMarks marks, IReadOnlyDictionary<uint, uint> newKeys)
    {
        var triangles = new List<WALL.Triangle>();

        foreach (var triangle in marks.Triangles)
        {
            if (!newKeys.TryGetValue(triangle.ShapeKey, out var key))
                continue;

            var copy = triangle.Clone();
            copy.ShapeKey = key;
            triangles.Add(copy);
        }

        var noJump = marks.NoJumpKeys
            .Where(newKeys.ContainsKey)
            .Select(k => newKeys[k])
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        return new WallMarks(triangles.OrderBy(t => t.ShapeKey).ToList(), noJump);
    }

    /// <summary>
    /// Replaces the marks of a collision with copies of the given ones, creating its files if it had none, and drops
    /// its cached triangles so the walls are displayed on its current shape.
    /// </summary>
    public void SetMarks(string mapId, string modelName, WallMarks marks)
    {
        var files = GetLoadedMapWalls(mapId);

        if (files == null)
            return;

        lock (files)
        {
            if (files.Files.TryGetValue(modelName, out var wall) || marks.Triangles.Count > 0)
            {
                if (wall == null)
                {
                    if (!long.TryParse(modelName.Substring(1), out var id))
                        return;

                    wall = new WALL { ID = id };
                    files.Files[modelName] = wall;
                }

                wall.Triangles = marks.Triangles.Select(t => t.Clone()).ToList();
                files.Modified.Add(modelName + WallExtension);
            }

            if (files.NoJumpFiles.TryGetValue(modelName, out var noJump) || marks.NoJumpKeys.Count > 0)
            {
                if (noJump == null)
                {
                    noJump = new NOJUMPWALL();
                    files.NoJumpFiles[modelName] = noJump;
                }

                noJump.ShapeKeys = new List<uint>(marks.NoJumpKeys);
                files.Modified.Add(modelName + NoJumpExtension);
            }
        }

        ForgetGeometry(mapId, modelName);
    }

    /// <summary>
    /// Drops the cached triangles of a collision, so they are read again from its document, e.g. after it has been
    /// rebuilt.
    /// </summary>
    public void ForgetGeometry(string mapId, string modelName)
    {
        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return;

        Geometry.TryRemove($"l{mapId.Substring(1)}_{modelName.Substring(1)}.hkx.dcx", out _);
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

        var document = Project.Handler.MapData.HavokCollisionBank.GetTagDocument(fileName);

        if (document == null)
            return null;

        var body = CollisionGeometry.Bodies(document.Objects)
            .FirstOrDefault(b => b.Shape is IHavokCompressedMesh);

        if (body == null)
            return null;

        var geometry = WallGeometry.Build((IHavokCompressedMesh)body.Shape, body);

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
                // e.g. h000100.wall.dcx and h000100.nojumpwall.dcx
                var fileName = GetFileName(file.Name);
                var name = fileName.Split('.')[0];

                var isWall = fileName.EndsWith(WallExtension, StringComparison.OrdinalIgnoreCase);
                var isNoJump = fileName.EndsWith(NoJumpExtension, StringComparison.OrdinalIgnoreCase);

                if (!isWall && !isNoJump)
                    continue;

                var data = DCX.Decompress(file.Bytes, out var compression);

                if (isWall)
                {
                    walls.Files[name] = WALL.Read(data);
                }
                else
                {
                    walls.NoJumpFiles[name] = NOJUMPWALL.Read(data);
                }

                walls.Compression[fileName] = compression;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Wall_Failed_To_Read", bhdPath), e);
        }

        return walls;
    }

    /// <summary>
    /// Writes the WALL and NOJUMPWALL files of a map that have been painted. Returns false if they couldn't be written.
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
                    foreach (var fileName in walls.Modified)
                    {
                        var name = fileName.Split('.')[0];
                        byte[] data;

                        if (fileName.EndsWith(NoJumpExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!walls.NoJumpFiles.TryGetValue(name, out var noJump))
                                continue;

                            // Collisions without any no-jump triangles have no file, rather than an empty one
                            if (noJump.ShapeKeys.Count == 0)
                            {
                                binder.Files.RemoveAll(e =>
                                    GetFileName(e.Name).Equals(fileName, StringComparison.OrdinalIgnoreCase));
                                continue;
                            }

                            data = noJump.Write();
                        }
                        else
                        {
                            if (!walls.Files.TryGetValue(name, out var wall))
                                continue;

                            data = wall.Write();
                        }

                        var compression = walls.Compression.GetValueOrDefault(fileName, DCX.Type.DCX_KRAK);
                        var bytes = DCX.Compress(data, compression);

                        var file = binder.Files.FirstOrDefault(e =>
                            GetFileName(e.Name).Equals(fileName, StringComparison.OrdinalIgnoreCase));

                        if (file != null)
                        {
                            file.Bytes = bytes;
                        }
                        else
                        {
                            var nextId = binder.Files.Count == 0 ? 0 : binder.Files.Max(e => e.ID) + 1;
                            binder.Files.Add(new BinderFile(Binder.FileFlags.Flag1, nextId, fileName, bytes));
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
