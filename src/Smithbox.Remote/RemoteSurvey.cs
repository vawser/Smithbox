using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using Veldrid.Utilities;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Surveying and scattering: rays against the render meshes (MeshScene, for place-on hit=mesh, los, labelVisible),
///     walkability with a capsule (capsule-path), passage profile (profile) and controlled irregular placement
///     (scatter).
/// </summary>
public static partial class RemoteControl
{
    #region Render-mesh rays

    private sealed class MeshItem
    {
        public MsbEntity Ent;
        public string Map, Model, Type;
        public Matrix4x4 World, Inv, InvT;
        public Vector3 LMin, LMax;
        public Vector3 WMin, WMax;
        public bool TreeTried;
        public FirstPersonTriangleTree Tree;
    }

    private struct MeshHit
    {
        public MeshItem Item;
        public Vector3 Point, Normal;
        public float Distance;
        public bool BoxOnly;
    }

    // FLVER triangle trees in model space, per map/model (objects: obj/model)
    private static readonly Dictionary<string, FirstPersonTriangleTree> MeshTrees = new(StringComparer.OrdinalIgnoreCase);

    private static FirstPersonTriangleTree MeshTree(ProjectEntry project, string map, string model)
    {
        if (string.IsNullOrEmpty(model) || model.StartsWith("c", StringComparison.OrdinalIgnoreCase))
            return null;

        var key = (model.StartsWith("o", StringComparison.OrdinalIgnoreCase) ? "obj" : map) + "/" + model;
        if (MeshTrees.TryGetValue(key, out var tree))
            return tree;

        var flver = ReadFlver(project, map, model);
        var tris = flver != null ? FlverTriangles(flver) : null;
        tree = tris is { Count: >= 3 } ? new FirstPersonTriangleTree(tris) : null;
        MeshTrees[key] = tree;
        return tree;
    }

    /// <summary>
    ///     The visible parts of the loaded maps with world matrix and box, for rays against their FLVER triangles
    ///     (trees are read on the first box hit and kept)
    /// </summary>
    private sealed class MeshScene
    {
        public readonly List<MeshItem> Items = new();
        private readonly ProjectEntry Project;

        public MeshScene(MapEditorView view, string onlyMap = null, HashSet<string> types = null)
        {
            Project = view.Editor.Project;
            types ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "MapPiece", "Object", "DummyObject" };

            foreach (var (map, container) in LoadedMaps(Project))
            {
                if (onlyMap != null && map != onlyMap)
                    continue;

                foreach (var ent in container.Objects.OfType<MsbEntity>())
                {
                    if (ent.WrappedObject is not IMsbPart mp || !ent.EditorVisible || !types.Contains(PartTypeName(ent)) ||
                        !TryLocalBounds(ent, out var lb))
                        continue;

                    var w = ent.GetWorldMatrix();
                    if (!Matrix4x4.Invert(w, out var inv))
                        continue;

                    var (wmin, wmax) = Extent(Corners(lb).Select(c => Vector3.Transform(c, w)));
                    Items.Add(new MeshItem
                    {
                        Ent = ent, Map = map, Model = mp.ModelName, Type = PartTypeName(ent), World = w, Inv = inv,
                        InvT = Matrix4x4.Transpose(inv), LMin = lb.Min, LMax = lb.Max, WMin = wmin, WMax = wmax,
                    });
                }
            }
        }

        public FirstPersonTriangleTree TreeOf(MeshItem it)
        {
            if (it.TreeTried)
                return it.Tree;

            it.TreeTried = true;
            it.Tree = MeshTree(Project, it.Map, it.Model);

            if (it.Tree == null && Origins.TryGetValue(it.Ent, out var origin) && origin[0] != it.Map)
                it.Tree = MeshTree(Project, origin[0], it.Model);

            return it.Tree;
        }

        public static bool Slab(Vector3 o, Vector3 d, Vector3 min, Vector3 max, float tMax, out float t0, out float t1)
        {
            t0 = 0;
            t1 = tMax;

            for (var i = 0; i < 3; i++)
            {
                var oi = Get(o, i);
                var di = Get(d, i);

                if (MathF.Abs(di) < 1e-12f)
                {
                    if (oi < Get(min, i) || oi > Get(max, i))
                        return false;
                    continue;
                }

                var a = (Get(min, i) - oi) / di;
                var b = (Get(max, i) - oi) / di;
                if (a > b)
                    (a, b) = (b, a);
                t0 = MathF.Max(t0, a);
                t1 = MathF.Min(t1, b);
                if (t0 > t1)
                    return false;
            }

            return true;
        }

        /// <summary>First hit on a render mesh; boxFallback: parts without a readable FLVER count with their box</summary>
        public bool RayCast(Vector3 origin, Vector3 dir, float max, Func<MeshItem, bool> skip, bool boxFallback, out MeshHit hit)
        {
            hit = default;
            dir = Vector3.Normalize(dir);
            var best = max;
            var found = false;

            foreach (var it in Items)
            {
                if (skip != null && skip(it))
                    continue;

                if (!Slab(origin, dir, it.WMin, it.WMax, best, out _, out _))
                    continue;

                var lo = Vector3.Transform(origin, it.Inv);
                var ld = Vector3.TransformNormal(dir, it.Inv);

                if (!Slab(lo, ld, it.LMin, it.LMax, best, out var t0, out _))
                    continue;

                var tree = TreeOf(it);

                if (tree == null)
                {
                    if (boxFallback && t0 < best)
                    {
                        best = t0;
                        found = true;
                        hit = new MeshHit { Item = it, Point = origin + dir * t0, Normal = -dir, Distance = t0, BoxOnly = true };
                    }

                    continue;
                }

                var len = ld.Length();
                if (len < 1e-12f || !tree.RayCast(lo, ld / len, best * len, out var dl, out var tri))
                    continue;

                var t = dl / len;
                if (t >= best)
                    continue;

                var n = Vector3.TransformNormal(tree.Normal(tri), it.InvT);
                n = n.LengthSquared() > 0 ? Vector3.Normalize(n) : -dir;
                if (Vector3.Dot(n, dir) > 0)
                    n = -n;

                best = t;
                found = true;
                hit = new MeshHit { Item = it, Point = origin + dir * t, Normal = n, Distance = t };
            }

            return found;
        }

        /// <summary>All parts whose triangles cross the segment (minT to max), each with its first intersection</summary>
        public List<MeshHit> Crossings(Vector3 origin, Vector3 dir, float minT, float max, Func<MeshItem, bool> skip)
        {
            dir = Vector3.Normalize(dir);
            var list = new List<MeshHit>();

            foreach (var it in Items)
            {
                if (skip != null && skip(it))
                    continue;

                if (!Slab(origin, dir, it.WMin, it.WMax, max, out _, out _))
                    continue;

                var lo = Vector3.Transform(origin, it.Inv);
                var ld = Vector3.TransformNormal(dir, it.Inv);

                if (!Slab(lo, ld, it.LMin, it.LMax, max, out var t0, out var t1) || t1 < minT)
                    continue;

                var tree = TreeOf(it);

                if (tree == null)
                {
                    if (t0 > minT)
                        list.Add(new MeshHit { Item = it, Point = origin + dir * t0, Distance = t0, BoxOnly = true });
                    continue;
                }

                var len = ld.Length();
                if (len < 1e-12f)
                    continue;

                var start = lo + ld * minT;
                if (tree.RayCast(start, ld / len, (max - minT) * len, out var dl, out _))
                {
                    var t = minT + dl / len;
                    list.Add(new MeshHit { Item = it, Point = origin + dir * t, Distance = t });
                }
            }

            return list.OrderBy(h => h.Distance).ToList();
        }
    }

    /// <summary>Ray against the map's render meshes, wrapped as a collision hit (for place-on hit=mesh)</summary>
    private static bool MeshCast(MeshScene scene, Vector3 origin, Vector3 dir, float max, ICollection<string> skip, RayHit result)
    {
        result.Origin = origin;
        result.Direction = dir;

        if (!scene.RayCast(origin, dir, max, it => skip != null && skip.Contains(it.Ent.Name), false, out var h))
            return false;

        result.Found = true;
        result.Hit = new FirstPersonHit
        {
            Point = h.Point, Normal = h.Normal, Distance = h.Distance, Material = FirstPersonTriangles.NoMaterial,
            Mesh = new FirstPersonMesh { PartName = h.Item.Ent.Name, MapId = h.Item.Map, Source = "render:" + h.Item.Model },
        };
        return true;
    }

    #endregion

    #region Shared geometry helpers

    /// <summary>Point list: JSON [[x,y,z],...] (also as text) or "x,y,z;x,y,z"; null if the argument is missing</summary>
    private static List<Vector3> GetPoints(JsonObject args, string name)
    {
        var node = args[name];
        if (node == null)
            return null;

        if (node is JsonValue && GetString(args, name) is string s)
        {
            s = s.Trim();
            if (s.StartsWith('['))
                node = JsonNode.Parse(s);
            else
                return s.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(p => ParseVec(p, name)).ToList();
        }

        if (node is not JsonArray arr)
            throw new RemoteException($"'{name}' must be a list of points [[x,y,z],...]");

        var list = new List<Vector3>();
        foreach (var e in arr)
        {
            if (e is JsonArray a && a.Count == 3)
                list.Add(new Vector3(ParseFloat(a[0], name), ParseFloat(a[1], name), ParseFloat(a[2], name)));
            else if (e is JsonObject o && GetVector(o, "pos") is Vector3 p)
                list.Add(p);
            else if (e is JsonValue v && v.TryGetValue<string>(out var t))
                list.Add(ParseVec(t, name));
            else
                throw new RemoteException($"'{name}': {e?.ToJsonString()} is not a point");
        }

        return list;
    }

    private static Vector3 ParseVec(string s, string name)
    {
        var v = s.Trim('[', ']', ' ').Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
                ? f
                : throw new RemoteException($"'{name}': '{s}' is not x,y,z")).ToArray();
        return v.Length == 3 ? new Vector3(v[0], v[1], v[2]) : throw new RemoteException($"'{name}': '{s}' is not x,y,z");
    }

    private static string F(float f) => f.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>Deepest penetration of a capsule (feet at the bottom) into the collision: depth, contact point, collision part</summary>
    private static (float Depth, Vector3 Point, string Part) CapsuleContact(FirstPersonCollisionSet set, Vector3 feet, float radius,
        float height)
    {
        var bottom = feet + new Vector3(0, radius, 0);
        var top = feet + new Vector3(0, MathF.Max(radius, height - radius), 0);
        var best = 0f;
        var point = feet;
        string part = null;

        set.Overlap(Vector3.Min(bottom, top) - new Vector3(radius), Vector3.Max(bottom, top) + new Vector3(radius), (mesh, tri, a, b, c) =>
        {
            var d = radius - MathF.Sqrt(FirstPersonCapsule.SegmentTriangle(bottom, top, a, b, c, out _, out var onTriangle));
            if (d > best)
            {
                best = d;
                point = onTriangle;
                part = mesh.PartName;
            }
        });

        return (best, point, part);
    }

    private static void ThickLine(Image<Rgba32> image, float x0, float y0, float x1, float y1, Rgba32 color, int width = 2)
    {
        for (var o = -(width / 2); o <= (width - 1) / 2; o++)
        {
            if (MathF.Abs(x1 - x0) > MathF.Abs(y1 - y0))
                ViewportLabels.Line(image, (int)x0, (int)y0 + o, (int)x1, (int)y1 + o, color);
            else
                ViewportLabels.Line(image, (int)x0 + o, (int)y0, (int)x1 + o, (int)y1, color);
        }
    }

    private static void Label(Image<Rgba32> image, int x, int y, string text, Rgba32 color, List<ViewportLabels.Box> used = null)
    {
        used ??= new List<ViewportLabels.Box>();
        if (ViewportLabels.TryPlace(used, text, x, y, 1, image.Width, image.Height, out var box))
            ViewportLabels.Text(image, box, text, color, new Rgba32(0, 0, 0), 0.65f, 1);
    }

    #endregion

    #region capsule-path

    private static IEnumerator CapsulePath(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var from = GetVector(args, "from") ?? throw new RemoteException("Missing argument 'from' (x,y,z)");
        var to = GetVector(args, "to");
        var toMin = GetVector(args, "toMin");
        var toMax = GetVector(args, "toMax");

        if (to == null && (toMin == null || toMax == null))
            throw new RemoteException("Give to=x,y,z or toMin=x,y,z toMax=x,y,z (target area)");

        var radius = GetFloat(args, "radius", 0.4f);
        var height = GetFloat(args, "height", 1.8f);
        var stepH = GetFloat(args, "stepHeight", 0.45f);
        var slopeDeg = GetFloat(args, "slope", 50);
        var res = MathF.Min(GetFloat(args, "step", 0.25f), radius);
        var narrow = GetFloat(args, "narrow", 0.15f);
        var maxNodes = (int)GetFloat(args, "maxNodes", 400000);
        var drop = GetFloat(args, "drop", 0);
        var pad = GetFloat(args, "pad", 8);

        var collision = EnsureCollision(view, 300);
        while (collision.MoveNext())
            yield return null;

        Vector3 gLo, gHi;
        if (to is Vector3 tp)
        {
            gLo = tp - new Vector3(res, 2.5f, res);
            gHi = tp + new Vector3(res, 1.5f, res);
        }
        else
        {
            gLo = Vector3.Min(toMin.Value, toMax.Value);
            gHi = Vector3.Max(toMin.Value, toMax.Value);
        }

        var lo = GetVector(args, "min") ?? Vector3.Min(from, gLo) - new Vector3(pad, 4, pad);
        var hi = GetVector(args, "max") ?? Vector3.Max(from, gHi) + new Vector3(pad, 4 + height, pad);
        (lo, hi) = (Vector3.Min(lo, hi), Vector3.Max(lo, hi));
        var nx = (int)MathF.Ceiling((hi.X - lo.X) / res) + 1;
        var nz = (int)MathF.Ceiling((hi.Z - lo.Z) / res) + 1;

        if ((long)nx * nz > maxNodes)
            throw new RemoteException($"Search area {nx} x {nz} cells, more than maxNodes={maxNodes}: give min=/max= or a larger step=");

        var set = new FirstPersonCollisionSet();
        view.FirstPersonCollision.Current.CopyNearby(lo - new Vector3(2), hi + new Vector3(2), set);

        var settings = FirstPersonMoveSettings.Default;
        settings.StepHeight = stepH;
        settings.MaxSlopeCos = MathF.Cos(slopeDeg * Deg);
        var tanSlope = MathF.Tan(slopeDeg * Deg);
        // The part above the step height: what is lower is stepped onto, as the walking controller does
        var upper = new FirstPersonCapsule(radius, MathF.Max(2 * radius + 0.01f, height - stepH));

        var floors = new Dictionary<long, float[]>();
        var fits = new Dictionary<(int, int, int), bool>();

        Vector3 Pos(int ix, int iz, float y) => new(lo.X + ix * res, y, lo.Z + iz * res);
        int YKey(float y) => (int)MathF.Round(y * 4);

        float[] FloorsAt(int ix, int iz)
        {
            var key = (long)ix * nz + iz;
            if (floors.TryGetValue(key, out var f))
                return f;

            var list = new List<float>();
            var o = new Vector3(lo.X + ix * res, hi.Y, lo.Z + iz * res);
            for (var k = 0; k < 24 && o.Y > lo.Y; k++)
            {
                if (!set.RayCast(o, -Vector3.UnitY, o.Y - lo.Y, out var h))
                    break;
                if (settings.CanStandOn(h))
                    list.Add(h.Point.Y);
                o = h.Point - new Vector3(0, 0.02f, 0);
            }

            return floors[key] = list.ToArray();
        }

        bool Fits(int ix, int iz, float y)
        {
            var k = (ix, iz, YKey(y));
            if (fits.TryGetValue(k, out var ok))
                return ok;

            ok = !upper.Overlaps(set, Pos(ix, iz, y + 0.02f + stepH), 0.02f, out _);
            fits[k] = ok;
            return ok;
        }

        float FloorNear(int ix, int iz, float y, float up, float down)
        {
            var best = float.NaN;
            foreach (var fy in FloorsAt(ix, iz))
            {
                var dy = fy - y;
                if (dy <= up && dy >= -down && (float.IsNaN(best) || MathF.Abs(dy) < MathF.Abs(best - y)))
                    best = fy;
            }

            return best;
        }

        bool IsGoal(Vector3 p) => p.X >= gLo.X && p.X <= gHi.X && p.Z >= gLo.Z && p.Z <= gHi.Z && p.Y >= gLo.Y && p.Y <= gHi.Y;

        float H(Vector3 p)
        {
            var c = Vector3.Clamp(p, gLo, gHi);
            return Vector3.Distance(p, c);
        }

        // A target point: the box around the floor below it (the point may be at eye height)
        if (to is Vector3 tg)
        {
            var gx = Math.Clamp((int)MathF.Round((tg.X - lo.X) / res), 0, nx - 1);
            var gz = Math.Clamp((int)MathF.Round((tg.Z - lo.Z) / res), 0, nz - 1);
            var gy = FloorsAt(gx, gz).Where(y => y <= tg.Y + 0.5f).DefaultIfEmpty(float.NaN).Max();
            if (!float.IsNaN(gy))
            {
                gLo.Y = gy - 0.5f;
                gHi.Y = gy + 0.5f;
                result["toFloor"] = Round(gy);
            }
        }

        // Start: the floor below from, in the nearest cell where the capsule fits
        var six = (int)MathF.Round((from.X - lo.X) / res);
        var siz = (int)MathF.Round((from.Z - lo.Z) / res);
        int startX = -1, startZ = -1;
        var startY = float.NaN;
        var ring = (int)MathF.Ceiling(2 * radius / res);

        for (var r = 0; r <= ring && float.IsNaN(startY); r++)
        {
            for (var dx = -r; dx <= r && float.IsNaN(startY); dx++)
            for (var dz = -r; dz <= r && float.IsNaN(startY); dz++)
            {
                if (Math.Max(Math.Abs(dx), Math.Abs(dz)) != r)
                    continue;
                int ix = six + dx, iz = siz + dz;
                if (ix < 0 || iz < 0 || ix >= nx || iz >= nz)
                    continue;
                var fy = FloorsAt(ix, iz).Where(y => y <= from.Y + 0.5f).DefaultIfEmpty(float.NaN).Max();
                if (!float.IsNaN(fy) && Fits(ix, iz, fy))
                    (startX, startZ, startY) = (ix, iz, fy);
            }
        }

        result["capsule"] = new JsonObject { ["radius"] = radius, ["height"] = height, ["stepHeight"] = stepH, ["slope"] = slopeDeg };

        if (float.IsNaN(startY))
        {
            var fy = FloorsAt(Math.Clamp(six, 0, nx - 1), Math.Clamp(siz, 0, nz - 1)).Where(y => y <= from.Y + 0.5f).DefaultIfEmpty(float.NaN).Max();
            result["found"] = false;
            result["reason"] = float.IsNaN(fy) ? "no walkable floor below from" : "the capsule does not fit at from";
            if (!float.IsNaN(fy))
            {
                var c = CapsuleContact(set, new Vector3(from.X, fy + 0.02f + stepH, from.Z), radius, upper.Height);
                result["contact"] = new JsonObject { ["part"] = c.Part, ["point"] = ToJson(c.Point), ["depth"] = Round(c.Depth) };
            }

            yield break;
        }

        // A* over the cells; a node is a floor of a cell where the capsule fits
        var nodes = new List<(int Ix, int Iz, float Y, float G, int Parent)> { (startX, startZ, startY, 0, -1) };
        var index = new Dictionary<(int, int, int), int> { [(startX, startZ, YKey(startY))] = 0 };
        var closed = new HashSet<int>();
        var open = new PriorityQueue<int, float>();
        open.Enqueue(0, H(Pos(startX, startZ, startY)));
        var dirs = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) };
        int goal = -1, bestNode = 0, expanded = 0;
        var bestH = float.MaxValue;

        while (open.TryDequeue(out var id, out _))
        {
            if (!closed.Add(id))
                continue;

            var n = nodes[id];
            var p = Pos(n.Ix, n.Iz, n.Y);

            if (IsGoal(p))
            {
                goal = id;
                break;
            }

            var hn = H(p);
            if (hn < bestH)
            {
                bestH = hn;
                bestNode = id;
            }

            if (++expanded > maxNodes)
                break;

            if (expanded % 1500 == 0)
                yield return null;

            foreach (var (dx, dz) in dirs)
            {
                int jx = n.Ix + dx, jz = n.Iz + dz;
                if (jx < 0 || jz < 0 || jx >= nx || jz >= nz)
                    continue;

                var diagonal = dx != 0 && dz != 0;
                var hd = res * (diagonal ? 1.41421f : 1f);
                var up = stepH + hd * tanSlope;
                var fy = FloorNear(jx, jz, n.Y, up, up + drop);

                if (float.IsNaN(fy) || !Fits(jx, jz, fy))
                    continue;

                // No cutting corners: both cells beside a diagonal step must be walkable too
                if (diagonal)
                {
                    var a = FloorNear(n.Ix + dx, n.Iz, n.Y, up, up + drop);
                    var b = FloorNear(n.Ix, n.Iz + dz, n.Y, up, up + drop);
                    if (float.IsNaN(a) || float.IsNaN(b) || !Fits(n.Ix + dx, n.Iz, a) || !Fits(n.Ix, n.Iz + dz, b))
                        continue;
                }

                var key = (jx, jz, YKey(fy));
                var g = n.G + MathF.Sqrt(hd * hd + (fy - n.Y) * (fy - n.Y));

                if (index.TryGetValue(key, out var jid))
                {
                    if (closed.Contains(jid) || nodes[jid].G <= g)
                        continue;
                    nodes[jid] = (jx, jz, fy, g, id);
                }
                else
                {
                    jid = nodes.Count;
                    nodes.Add((jx, jz, fy, g, id));
                    index[key] = jid;
                }

                open.Enqueue(jid, g + H(Pos(jx, jz, fy)));
            }
        }

        result["explored"] = expanded;
        result["cells"] = new JsonArray(nx, nz);

        // Room beside the capsule: rays across the walking direction (45 to 135 degrees off it), so steps ahead or behind
        // don't count as narrow
        float Clearance(Vector3 feet, Vector3 along, out string part, out Vector3 point)
        {
            var best = radius + 2f;
            part = null;
            point = feet;
            var baseAngle = MathF.Atan2(along.X, along.Z);
            foreach (var hy in new[] { stepH + 0.25f, height * 0.5f, height - 0.1f })
            {
                foreach (var off in new[] { 45f, 67.5f, 90f, 112.5f, 135f, -45f, -67.5f, -90f, -112.5f, -135f })
                {
                    var a = baseAngle + off * Deg;
                    if (set.RayCast(feet + new Vector3(0, hy, 0), new Vector3(MathF.Sin(a), 0, MathF.Cos(a)), best, out var h) && h.Distance < best)
                    {
                        best = h.Distance;
                        part = h.Mesh?.PartName;
                        point = h.Point;
                    }
                }
            }

            return best - radius;
        }

        (float Dist, string Part) Side(Vector3 feet, Vector3 dir)
        {
            var best = 20f;
            string part = null;
            foreach (var hy in new[] { stepH + 0.25f, height * 0.5f })
            {
                if (set.RayCast(feet + new Vector3(0, hy, 0), dir, best, out var h) && h.Distance < best)
                {
                    best = h.Distance;
                    part = h.Mesh?.PartName;
                }
            }

            return (best, part);
        }

        var feetPath = new List<Vector3>();
        var drawNarrow = new List<(Vector3 At, string Text)>();
        Vector3? stuckAt = null;

        if (goal >= 0)
        {
            for (var i = goal; i >= 0; i = nodes[i].Parent)
                feetPath.Add(Pos(nodes[i].Ix, nodes[i].Iz, nodes[i].Y));
            feetPath.Reverse();

            var length = 0f;
            var maxRise = 0f;
            for (var i = 1; i < feetPath.Count; i++)
            {
                length += Vector3.Distance(feetPath[i - 1], feetPath[i]);
                maxRise = MathF.Max(maxRise, MathF.Abs(feetPath[i].Y - feetPath[i - 1].Y));
            }

            // Narrow places: nodes where the room beside the capsule is below narrow, grouped
            var clear = new List<(float c, string part, Vector3 pt)>();
            for (var i = 0; i < feetPath.Count; i++)
            {
                var a = feetPath[Math.Max(0, i - 2)];
                var b = feetPath[Math.Min(feetPath.Count - 1, i + 2)];
                var along = new Vector3(b.X - a.X, 0, b.Z - a.Z);
                var c = Clearance(feetPath[i], along.LengthSquared() > 1e-6f ? Vector3.Normalize(along) : Vector3.UnitZ, out var part, out var pt);
                clear.Add((c, part, pt));
            }
            var narrowList = new JsonArray();
            var minClear = clear.Count > 0 ? clear.Min(c => c.c) : 0;

            for (var i = 0; i < clear.Count; i++)
            {
                if (clear[i].c >= narrow)
                    continue;

                var j = i;
                while (j + 1 < clear.Count && clear[j + 1].c < narrow)
                    j++;

                var k = Enumerable.Range(i, j - i + 1).OrderBy(x => clear[x].c).First();
                var a = feetPath[Math.Max(0, k - 2)];
                var b = feetPath[Math.Min(feetPath.Count - 1, k + 2)];
                var d = new Vector3(b.X - a.X, 0, b.Z - a.Z);
                d = d.LengthSquared() > 1e-6f ? Vector3.Normalize(d) : Vector3.UnitZ;
                var right = new Vector3(d.Z, 0, -d.X);
                var l = Side(feetPath[k], -right);
                var r = Side(feetPath[k], right);
                var span = 0f;
                for (var x = i + 1; x <= j; x++)
                    span += Vector3.Distance(feetPath[x - 1], feetPath[x]);

                narrowList.Add(new JsonObject
                {
                    ["at"] = ToJson(feetPath[k]),
                    ["clearance"] = Round(clear[k].c),
                    ["width"] = Round(l.Dist + r.Dist),
                    ["left"] = new JsonObject { ["dist"] = Round(l.Dist), ["part"] = l.Part },
                    ["right"] = new JsonObject { ["dist"] = Round(r.Dist), ["part"] = r.Part },
                    ["nearest"] = new JsonObject { ["part"] = clear[k].part, ["point"] = ToJson(clear[k].pt) },
                    ["length"] = Round(span),
                });
                drawNarrow.Add((feetPath[k], $"{F(l.Dist + r.Dist)} m"));
                i = j;
            }

            var points = new JsonArray();
            var since = 1e9f;
            for (var i = 0; i < feetPath.Count; i++)
            {
                since += i > 0 ? Vector3.Distance(feetPath[i - 1], feetPath[i]) : 0;
                if (since >= 0.5f || i == feetPath.Count - 1)
                {
                    points.Add(ToJson(feetPath[i]));
                    since = 0;
                }
            }

            result["found"] = true;
            result["length"] = Round(length);
            result["maxStep"] = Round(maxRise);
            result["minClearance"] = Round(minClear);
            result["path"] = points;
            result["narrow"] = narrowList;

            // Walk it with Smithbox's first person controller (the same capsule): where does it get stuck?
            if (GetBool(args, "simulate", true))
            {
                var waypoints = new List<Vector3>();
                var acc = 0f;
                for (var i = 1; i < feetPath.Count; i++)
                {
                    acc += Vector3.Distance(feetPath[i - 1], feetPath[i]);
                    if (acc >= 1f || i == feetPath.Count - 1)
                    {
                        waypoints.Add(feetPath[i]);
                        acc = 0;
                    }
                }

                var ctrl = new FirstPersonController(new FirstPersonCapsule(radius, height), settings);
                ctrl.Reset(feetPath[0] + new Vector3(0, 0.05f, 0));
                var status = "arrived";
                var wi = 0;
                var steps = 0;
                var bestD = float.MaxValue;
                var sinceBest = 0;

                while (wi < waypoints.Count)
                {
                    var target = waypoints[wi];
                    var d = target - ctrl.Feet;
                    d.Y = 0;
                    var dist = d.Length();

                    if (dist < 0.3f)
                    {
                        wi++;
                        bestD = float.MaxValue;
                        sinceBest = 0;
                        continue;
                    }

                    if (dist < bestD - 0.03f)
                    {
                        bestD = dist;
                        sinceBest = 0;
                    }
                    else if (++sinceBest > 60)
                    {
                        status = "stuck";
                        break;
                    }

                    ctrl.Step(set, new FirstPersonInput { Move = d / dist });

                    if (ctrl.Feet.Y < target.Y - 4)
                    {
                        status = "fell";
                        break;
                    }

                    if (++steps > 30000)
                    {
                        status = "aborted";
                        break;
                    }

                    if (steps % 300 == 0)
                        yield return null;
                }

                var sim = new JsonObject
                {
                    ["status"] = status, ["steps"] = steps, ["seconds"] = Round(steps / 60f), ["at"] = ToJson(ctrl.Feet),
                    ["waypoint"] = wi, ["waypoints"] = waypoints.Count,
                };

                if (status != "arrived")
                {
                    var c = CapsuleContact(set, ctrl.Feet + new Vector3(0, 0.01f, 0), radius + 0.05f, height);
                    sim["contact"] = new JsonObject { ["part"] = c.Part, ["point"] = ToJson(c.Point), ["depth"] = Round(c.Depth) };
                    stuckAt = ctrl.Feet;
                }

                result["simulate"] = sim;
            }
        }
        else
        {
            // No way: from the node nearest the goal towards it, what stops the capsule
            var n = nodes[bestNode];
            var p = Pos(n.Ix, n.Iz, n.Y);
            var target = Vector3.Clamp(p, gLo, gHi);
            var d = new Vector3(target.X - p.X, 0, target.Z - p.Z);
            var blocked = new JsonObject { ["reason"] = expanded > maxNodes ? "maxNodes reached" : "goal not reachable" };

            if (d.LengthSquared() > 1e-6f)
            {
                d = Vector3.Normalize(d);
                var y = p.Y;
                for (var s = 0.1f; s <= 3f; s += 0.1f)
                {
                    var q = p + d * s;
                    var up = stepH + 0.1f * tanSlope;
                    var floorOk = set.RayCast(new Vector3(q.X, y + up + 0.1f, q.Z), -Vector3.UnitY, 2 * up + 0.3f + drop, out var fh);

                    if (!floorOk)
                    {
                        blocked["reason"] = "no floor";
                        blocked["point"] = ToJson(new Vector3(q.X, y, q.Z));
                        break;
                    }

                    if (!settings.CanStandOn(fh))
                    {
                        blocked["reason"] = "too steep or slippery";
                        blocked["point"] = ToJson(fh.Point);
                        blocked["part"] = fh.Mesh?.PartName;
                        break;
                    }

                    var c = CapsuleContact(set, new Vector3(q.X, fh.Point.Y + 0.02f + stepH, q.Z), radius, upper.Height);
                    if (c.Depth > 0.02f)
                    {
                        blocked["reason"] = "capsule collides";
                        blocked["point"] = ToJson(c.Point);
                        blocked["part"] = c.Part;
                        blocked["depth"] = Round(c.Depth);
                        break;
                    }

                    y = fh.Point.Y;
                }
            }

            result["found"] = false;
            result["closest"] = ToJson(p);
            result["closestDistance"] = Round(bestH);
            result["blocked"] = blocked;
            stuckAt = GetVector(blocked, "point") ?? p;
            feetPath.Add(Pos(startX, startZ, startY));
            for (var i = bestNode; i >= 0; i = nodes[i].Parent)
                feetPath.Insert(1, Pos(nodes[i].Ix, nodes[i].Iz, nodes[i].Y));
        }

        // Top view with the way, the narrow places and where it stops
        if (GetString(args, "path") is string png)
        {
            var all = feetPath.Concat(new[] { from, to ?? (gLo + gHi) / 2 }).ToList();
            var margin = GetFloat(args, "margin", 3);
            var pmin = new Vector2(all.Min(v => v.X) - margin, all.Min(v => v.Z) - margin);
            var pmax = new Vector2(all.Max(v => v.X) + margin, all.Max(v => v.Z) + margin);
            var cut = GetFloat(args, "cut", all.Max(v => v.Y) + height + 0.3f);
            var planArgs = new JsonObject
            {
                ["min"] = $"{F(pmin.X)},{F(pmin.Y)}", ["max"] = $"{F(pmax.X)},{F(pmax.Y)}", ["cut"] = cut, ["path"] = png,
                ["gap"] = GetBool(args, "gap", true), ["settle"] = GetFloat(args, "settle", 4),
                ["depth"] = GetFloat(args, "depth", 30),
            };
            var plan = new JsonObject();
            var sub = RunSub("plan", planArgs, plan);
            while (sub.MoveNext())
                yield return null;

            var mpp = (float)(double)plan["metersPerPixel"];
            var ox = (float)(double)plan["origin"]![0];
            var oz = (float)(double)plan["origin"]![1];
            (float X, float Y) Px(Vector3 v) => ((v.X - ox) / mpp, (oz - v.Z) / mpp);

            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(png))
            {
                var used = new List<ViewportLabels.Box>();
                var green = new Rgba32(60, 230, 90);
                var lineColor = goal >= 0 ? green : new Rgba32(255, 170, 40);
                for (var i = 1; i < feetPath.Count; i++)
                {
                    var (x0, y0) = Px(feetPath[i - 1]);
                    var (x1, y1) = Px(feetPath[i]);
                    ThickLine(image, x0, y0, x1, y1, lineColor, 3);
                }

                var (ax, ay) = Px(from);
                ViewportLabels.Marker(image, (int)ax, (int)ay, green);
                Label(image, (int)ax, (int)ay, "A", green, used);
                var (bx, by) = Px(to ?? (gLo + gHi) / 2);
                ViewportLabels.Marker(image, (int)bx, (int)by, new Rgba32(80, 160, 255));
                Label(image, (int)bx, (int)by, "B", new Rgba32(80, 160, 255), used);

                foreach (var (at, text) in drawNarrow)
                {
                    var (x, y) = Px(at);
                    ViewportLabels.Marker(image, (int)x, (int)y, new Rgba32(255, 60, 60));
                    Label(image, (int)x, (int)y, text, new Rgba32(255, 90, 90), used);
                }

                if (stuckAt is Vector3 sa)
                {
                    var (x, y) = Px(sa);
                    var m = new Rgba32(255, 0, 255);
                    ThickLine(image, x - 8, y - 8, x + 8, y + 8, m, 3);
                    ThickLine(image, x - 8, y + 8, x + 8, y - 8, m, 3);
                    Label(image, (int)x, (int)y, goal >= 0 ? "stuck" : "blocked", m, used);
                }

                image.SaveAsPng(png);
            }

            result["image"] = png;
            result["metersPerPixel"] = plan["metersPerPixel"]?.DeepClone();
            result["origin"] = plan["origin"]?.DeepClone();
        }
    }

    #endregion

    #region profile

    private static IEnumerator Profile(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var pts = GetPoints(args, "points");

        if (pts == null)
        {
            var a = GetVector(args, "from") ?? throw new RemoteException("Give points=[[x,y,z],...] or from=x,y,z to=x,y,z");
            var b = GetVector(args, "to") ?? throw new RemoteException("Missing argument 'to'");
            pts = new List<Vector3> { a, b };
        }

        if (pts.Count < 2)
            throw new RemoteException("A profile needs at least two points");

        var stepLen = MathF.Max(0.05f, GetFloat(args, "step", 1));
        var probe = GetFloat(args, "probe", 1.0f);
        var heights = (GetString(args, "heights") ?? "0.5,1.5").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        var maxRay = GetFloat(args, "max", 30);

        var collision = EnsureCollision(view, 300);
        while (collision.MoveNext())
            yield return null;

        var set = view.FirstPersonCollision.Current;

        // Stations every step along the path (plan length)
        var stations = new List<(float S, Vector3 P, Vector3 Dir)>();
        var total = 0f;
        var next = 0f;
        for (var i = 0; i + 1 < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[i + 1];
            var flat = new Vector3(b.X - a.X, 0, b.Z - a.Z);
            var len = flat.Length();
            if (len < 1e-4f)
                continue;
            var dir = flat / len;
            while (next <= total + len + 1e-4f)
            {
                var f = (next - total) / len;
                stations.Add((next, Vector3.Lerp(a, b, f), dir));
                next += stepLen;
            }

            total += len;
        }

        if (stations.Count == 0 || stations[^1].S < total - 1e-3f)
        {
            var a = pts[^2];
            var b = pts[^1];
            var flat = new Vector3(b.X - a.X, 0, b.Z - a.Z);
            stations.Add((total, b, flat.LengthSquared() > 0 ? Vector3.Normalize(flat) : Vector3.UnitZ));
        }

        var rows = new List<(float S, Vector3 P, float Floor, float Left, float Right, float Head, float Slope, string PL, string PR, string PH, string PF)>();
        float prevFloor = float.NaN, prevS = 0;

        foreach (var (s, p, d) in stations)
        {
            var floor = float.NaN;
            string pf = null;
            if (set.RayCast(p + new Vector3(0, probe, 0), -Vector3.UnitY, probe + 20, out var fh))
            {
                floor = fh.Point.Y;
                pf = fh.Mesh?.PartName;
            }

            var baseY = float.IsNaN(floor) ? p.Y : floor;
            var basePt = new Vector3(p.X, baseY, p.Z);
            var right = new Vector3(d.Z, 0, -d.X);
            float left = maxRay, rightD = maxRay, head = maxRay;
            string pl = null, pr = null, ph = null;

            foreach (var hy in heights)
            {
                if (set.RayCast(basePt + new Vector3(0, hy, 0), -right, left, out var lh) && lh.Distance < left)
                {
                    left = lh.Distance;
                    pl = lh.Mesh?.PartName;
                }

                if (set.RayCast(basePt + new Vector3(0, hy, 0), right, rightD, out var rh) && rh.Distance < rightD)
                {
                    rightD = rh.Distance;
                    pr = rh.Mesh?.PartName;
                }
            }

            if (set.RayCast(basePt + new Vector3(0, 0.05f, 0), Vector3.UnitY, maxRay, out var hh))
            {
                head = hh.Distance + 0.05f;
                ph = hh.Mesh?.PartName;
            }

            var slope = !float.IsNaN(prevFloor) && !float.IsNaN(floor) && s > prevS ? (floor - prevFloor) / (s - prevS) * 100 : float.NaN;
            rows.Add((s, p, floor, left, rightD, head, slope, pl, pr, ph, pf));
            if (!float.IsNaN(floor))
            {
                prevFloor = floor;
                prevS = s;
            }
        }

        static JsonNode N(float f) => float.IsNaN(f) ? null : Round(f);

        result["stations"] = rows.Count;
        result["length"] = Round(total);
        result["rows"] = new JsonArray(rows.Select(r => (JsonNode)new JsonObject
        {
            ["s"] = Round(r.S), ["x"] = Round(r.P.X), ["z"] = Round(r.P.Z), ["floor"] = N(r.Floor),
            ["left"] = r.Left >= maxRay ? null : Round(r.Left), ["right"] = r.Right >= maxRay ? null : Round(r.Right),
            ["width"] = r.Left >= maxRay || r.Right >= maxRay ? null : Round(r.Left + r.Right),
            ["head"] = r.Head >= maxRay ? null : Round(r.Head), ["slope"] = N(r.Slope),
            ["leftPart"] = r.PL, ["rightPart"] = r.PR, ["headPart"] = r.PH, ["floorPart"] = r.PF,
        }).ToArray());

        string Col(float f, int w, bool open = false) =>
            (float.IsNaN(f) || (open && f >= maxRay) ? "-" : f.ToString("0.00", CultureInfo.InvariantCulture)).PadLeft(w);

        var table = new JsonArray { "     s       x       z   floor   left  right   width  head    slope%" };
        foreach (var r in rows)
        {
            table.Add(Col(r.S, 6) + Col(r.P.X, 8) + Col(r.P.Z, 8) + Col(r.Floor, 8) + Col(r.Left, 7, true) + Col(r.Right, 7, true) +
                      Col(r.Left >= maxRay || r.Right >= maxRay ? float.NaN : r.Left + r.Right, 8) + Col(r.Head, 6, true) +
                      Col(r.Slope, 10));
        }

        result["table"] = table;
        var widths = rows.Where(r => r.Left < maxRay && r.Right < maxRay).Select(r => r.Left + r.Right).ToList();
        var heads = rows.Where(r => r.Head < maxRay).Select(r => r.Head).ToList();
        result["minWidth"] = widths.Count > 0 ? Round(widths.Min()) : null;
        result["minHead"] = heads.Count > 0 ? Round(heads.Min()) : null;
        result["maxSlope"] = rows.Any(r => !float.IsNaN(r.Slope)) ? Round(rows.Where(r => !float.IsNaN(r.Slope)).Max(r => MathF.Abs(r.Slope))) : null;

        if (GetString(args, "path") is string png)
        {
            DrawProfileChart(png, rows.Select(r => (r.S, r.Floor, r.Left >= maxRay ? float.NaN : r.Left,
                    r.Right >= maxRay ? float.NaN : r.Right, r.Head >= maxRay ? float.NaN : r.Head)).ToList(),
                GetFloat(args, "minWidth", float.NaN), GetFloat(args, "minHead", float.NaN));
            result["image"] = png;
        }
    }

    private static float NiceStep(float range, int ticks)
    {
        var raw = range / MathF.Max(1, ticks);
        var p = MathF.Pow(10, MathF.Floor(MathF.Log10(MathF.Max(raw, 1e-6f))));
        foreach (var m in new[] { 1f, 2f, 5f, 10f })
            if (raw <= m * p)
                return m * p;
        return 10 * p;
    }

    /// <summary>Passage profile as an image: top the section (floor, ceiling), bottom the unrolled plan (left up, right down)</summary>
    private static void DrawProfileChart(string path, List<(float S, float Floor, float Left, float Right, float Head)> rows,
        float minWidth, float minHead)
    {
        const int W = 1200, Hgt = 760, X0 = 70, X1 = W - 30;
        using var image = new Image<Rgba32>(W, Hgt, new Rgba32(28, 28, 32));
        var grid = new Rgba32(70, 70, 80);
        var text = new Rgba32(220, 220, 220);
        var sMax = MathF.Max(rows.Max(r => r.S), 0.1f);
        float Sx(float s) => X0 + s / sMax * (X1 - X0);

        void Panel(int top, int bottom, float vMin, float vMax, string title, string unit)
        {
            if (vMax - vMin < 0.1f)
                vMax = vMin + 0.1f;
            Label(image, X0, top - 4, title, text);
            var ys = NiceStep(vMax - vMin, 6);
            for (var v = MathF.Ceiling(vMin / ys) * ys; v <= vMax; v += ys)
            {
                var y = bottom - (v - vMin) / (vMax - vMin) * (bottom - top);
                ViewportLabels.Line(image, X0, (int)y, X1, (int)y, grid);
                ViewportLabels.Text(image, new ViewportLabels.Box(2, (int)y - 8, 66, 17), F(v) + unit, text, new Rgba32(0, 0, 0), 0f, 1);
            }

            var xs = NiceStep(sMax, 10);
            for (var s = 0f; s <= sMax + 1e-3f; s += xs)
            {
                var x = Sx(s);
                ViewportLabels.Line(image, (int)x, top, (int)x, bottom, grid);
                ViewportLabels.Text(image, new ViewportLabels.Box((int)x - 10, bottom + 3, 60, 17), F(s), text, new Rgba32(0, 0, 0), 0f, 1);
            }
        }

        void Curve(Func<(float S, float Floor, float Left, float Right, float Head), float> value, int top, int bottom, float vMin, float vMax,
            Rgba32 color)
        {
            if (vMax - vMin < 0.1f)
                vMax = vMin + 0.1f;
            float? px = null, py = null;
            foreach (var r in rows)
            {
                var v = value(r);
                if (float.IsNaN(v))
                {
                    px = py = null;
                    continue;
                }

                var x = Sx(r.S);
                var y = bottom - (Math.Clamp(v, vMin, vMax) - vMin) / (vMax - vMin) * (bottom - top);
                if (px != null)
                    ThickLine(image, px.Value, py!.Value, x, y, color, 2);
                else
                    ViewportLabels.Marker(image, (int)x, (int)y, color);
                (px, py) = (x, y);
            }
        }

        // Section: floor and ceiling (absolute height)
        var fl = rows.Where(r => !float.IsNaN(r.Floor)).ToList();
        if (fl.Count > 0)
        {
            var ceil = fl.Where(r => !float.IsNaN(r.Head)).Select(r => r.Floor + r.Head).DefaultIfEmpty(fl.Max(r => r.Floor) + 3).Max();
            var vMin = fl.Min(r => r.Floor) - 0.5f;
            var vMax = MathF.Max(ceil, fl.Max(r => r.Floor) + 1) + 0.5f;
            Panel(40, 330, vMin, vMax, "Section: floor (white), ceiling (cyan), height in m over s (m)", "");
            Curve(r => r.Floor, 40, 330, vMin, vMax, new Rgba32(240, 240, 240));
            Curve(r => float.IsNaN(r.Head) || float.IsNaN(r.Floor) ? float.NaN : r.Floor + r.Head, 40, 330, vMin, vMax, new Rgba32(80, 220, 255));
            if (!float.IsNaN(minHead))
                foreach (var r in rows.Where(r => !float.IsNaN(r.Head) && r.Head < minHead && !float.IsNaN(r.Floor)))
                    ViewportLabels.Marker(image, (int)Sx(r.S), (int)(330 - (r.Floor + r.Head - vMin) / (vMax - vMin) * 290), new Rgba32(255, 60, 60));
        }

        // Unrolled plan: left wall up, right wall down, the path in the middle
        var maxL = rows.Where(r => !float.IsNaN(r.Left)).Select(r => r.Left).DefaultIfEmpty(1).Max();
        var maxR = rows.Where(r => !float.IsNaN(r.Right)).Select(r => r.Right).DefaultIfEmpty(1).Max();
        var lim = MathF.Max(maxL, maxR) + 0.3f;
        Panel(420, 720, -lim, lim, "Unrolled plan: left (up, yellow), right (down, orange), path = 0", "");
        ViewportLabels.Line(image, X0, 570, X1, 570, new Rgba32(60, 230, 90));
        Curve(r => r.Left, 420, 720, -lim, lim, new Rgba32(255, 220, 60));
        Curve(r => float.IsNaN(r.Right) ? float.NaN : -r.Right, 420, 720, -lim, lim, new Rgba32(255, 150, 50));
        if (!float.IsNaN(minWidth))
            foreach (var r in rows.Where(r => !float.IsNaN(r.Left) && !float.IsNaN(r.Right) && r.Left + r.Right < minWidth))
                ViewportLabels.Marker(image, (int)Sx(r.S), 570, new Rgba32(255, 60, 60));

        image.SaveAsPng(path);
    }

    #endregion

    #region scatter

    private static IEnumerator Scatter(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var (lo, hi) = Region3(args);
        var count = (int)GetFloat(args, "count", 5);
        var seed = (int)GetFloat(args, "seed", 1);
        var dry = GetBool(args, "dry", false);
        var keepIds = GetBool(args, "keepEntityId", false);
        var addAtlas = GetBool(args, "addAtlas", false);
        var wall = GetBool(args, "wall", false);
        var baseYaw = GetFloat(args, "yaw", 0);
        var yawSpread = GetFloat(args, "yawSpread", wall ? 20 : 360);
        var tilt = GetFloat(args, "tilt", 0);
        var scaleMin = GetFloat(args, "scaleMin", 1);
        var scaleMax = GetFloat(args, "scaleMax", scaleMin);
        var minDist = GetFloat(args, "minDist", 0);
        var clearance = GetFloat(args, "clearance", 0.05f);
        var wallGap = GetFloat(args, "wallGap", 0.02f);
        var wallRange = GetFloat(args, "wallRange", 1.5f);
        var avoid = GetBool(args, "avoid", true);
        var (axisIdx, axisSign) = ParseSide(GetString(args, "axis", "+z"), "axis");
        if (axisIdx == 1)
            throw new RemoteException("axis must be horizontal (+x, -x, +z, -z)");

        var palette = (GetString(args, "palette") ?? GetString(args, "from") ??
                       throw new RemoteException("Give palette=<map>/<part>,<map>/<part>*2,... (parts from original maps)"))
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p =>
            {
                var star = p.LastIndexOf('*');
                var w = star > 0 ? float.Parse(p[(star + 1)..], CultureInfo.InvariantCulture) : 1f;
                return (From: (star > 0 ? p[..star] : p).Trim(), Weight: w);
            }).ToList();

        var loadedHere = new List<string>();
        foreach (var map in palette.Select(p => SplitFrom(p.From).Map).Distinct())
        {
            var loaded = new bool[1];
            var load = EnsureMapLoaded(view, map, args, loaded);
            while (load.MoveNext())
                yield return null;
            if (loaded[0])
                loadedHere.Add(map);
        }

        try
        {
            var collision = EnsureCollision(view, 300);
            while (collision.MoveNext())
                yield return null;

            var set = view.FirstPersonCollision.Current;
            var sources = palette.Select(p =>
            {
                var (m, n) = SplitFrom(p.From);
                var ent = RequirePart(RequireLoadedMap(view, m), n);
                if (!TryLocalBounds(ent, out var lb))
                    throw new RemoteException($"{p.From}: model not loaded yet (no render mesh bounds), see /wait");
                return (Ent: ent, Map: m, Name: n, Box: lb, Weight: p.Weight, CanScale: HasScale(ent));
            }).ToList();
            var totalWeight = sources.Sum(s => s.Weight);

            // Existing small parts in the area as obstacles (render boxes), as free-spot
            var obstacles = avoid
                ? target.Objects.OfType<MsbEntity>()
                    .Where(e => e.WrappedObject is IMsbPart && PartTypeName(e) is "Object" or "MapPiece" or "Enemy" or "DummyObject" &&
                                e.RenderSceneMesh != null)
                    .Select(e => e.RenderSceneMesh.GetBounds())
                    .Where(b => (b.Max - b.Min).X < 3 && (b.Max - b.Min).Z < 3 &&
                                BoxesOverlap(b.Min, b.Max, lo - new Vector3(3), hi + new Vector3(3)))
                    .Select(b => (b.Min, b.Max)).ToList()
                : new List<(Vector3 Min, Vector3 Max)>();

            var rng = new Random(seed);
            var plan = new List<(int Src, Vector3 Pos, Quaternion Q, Vector3 Rot, Vector3 Scale, Vector3 BMin, Vector3 BMax, float Foot, string Wall)>();
            var tries = 0;
            var maxTries = (int)GetFloat(args, "maxTries", count * 80);
            var rejected = new Dictionary<string, int>();
            void Reject(string why) => rejected[why] = rejected.GetValueOrDefault(why) + 1;

            while (plan.Count < count && tries++ < maxTries)
            {
                if (tries % 40 == 0)
                    yield return null;

                // Weighted pick from the palette
                var pick = rng.NextSingle() * totalWeight;
                var si = 0;
                while (si < sources.Count - 1 && (pick -= sources[si].Weight) > 0)
                    si++;
                var src = sources[si];

                var x = lo.X + rng.NextSingle() * (hi.X - lo.X);
                var z = lo.Z + rng.NextSingle() * (hi.Z - lo.Z);
                if (!set.RayCast(new Vector3(x, hi.Y, z), -Vector3.UnitY, hi.Y - lo.Y, out var floor) || floor.Normal.Y < 0.7f)
                {
                    Reject("no level floor");
                    continue;
                }

                var s = scaleMin + rng.NextSingle() * (scaleMax - scaleMin);
                var scale = src.CanScale ? new Vector3(s) : Vector3.One;
                var yaw = baseYaw + (rng.NextSingle() * 2 - 1) * yawSpread / 2;
                var c = floor.Point;
                string wallPart = null;
                var wallN = Vector3.Zero;

                if (wall)
                {
                    // Nearest wall around the point, the front (axis) away from it
                    var best = wallRange;
                    FirstPersonHit wh = default;
                    for (var k = 0; k < 16; k++)
                    {
                        var a = k * MathF.PI / 8;
                        if (set.RayCast(c + new Vector3(0, 0.5f, 0), new Vector3(MathF.Sin(a), 0, MathF.Cos(a)), best, out var h) &&
                            MathF.Abs(h.Normal.Y) < 0.5f && h.Distance < best)
                        {
                            best = h.Distance;
                            wh = h;
                        }
                    }

                    if (wh.Mesh == null)
                    {
                        Reject("no wall in wallRange");
                        continue;
                    }

                    wallN = Vector3.Normalize(new Vector3(wh.Normal.X, 0, wh.Normal.Z));
                    var av = AxisVector(axisIdx, axisSign);
                    yaw = (MathF.Atan2(wallN.X, wallN.Z) - MathF.Atan2(av.X, av.Z)) / Deg + (rng.NextSingle() * 2 - 1) * yawSpread / 2;
                    wallPart = wh.Mesh.PartName;
                    c = new Vector3(wh.Point.X, c.Y, wh.Point.Z);
                }

                var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw * Deg);
                if (tilt > 0)
                {
                    var ta = rng.NextSingle() * MathF.PI * 2;
                    q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(new Vector3(MathF.Cos(ta), 0, MathF.Sin(ta)),
                        rng.NextSingle() * tilt * Deg));
                }

                var corners = PlacedCorners(src.Box, q, scale).ToList();
                var (bmin, bmax) = Extent(corners);

                if (wall)
                {
                    // Back of the box against the wall (gap), then onto the floor there
                    var back = corners.Min(k => Vector3.Dot(k, wallN));
                    var xz = c + wallN * (wallGap - back);
                    if (!set.RayCast(new Vector3(xz.X, c.Y + 1.0f, xz.Z), -Vector3.UnitY, 2.0f, out var f2) || f2.Normal.Y < 0.7f)
                    {
                        Reject("no floor at the wall");
                        continue;
                    }

                    c = f2.Point;
                    c.X = xz.X;
                    c.Z = xz.Z;
                }

                var pos = new Vector3(c.X, c.Y - bmin.Y, c.Z);
                var wmin = pos + bmin;
                var wmax = pos + bmax;
                var foot = MathF.Max(bmax.X - bmin.X, bmax.Z - bmin.Z) / 2;

                // Flat under the four bottom corners of the box
                var flat = true;
                foreach (var (u, v) in new[] { (0, 0), (0, 1), (1, 0), (1, 1) })
                {
                    var px = u == 0 ? wmin.X : wmax.X;
                    var pz = v == 0 ? wmin.Z : wmax.Z;
                    var inner = new Vector3(px + (pos.X - px) * 0.15f, c.Y + 0.5f, pz + (pos.Z - pz) * 0.15f);
                    if (!set.RayCast(inner, -Vector3.UnitY, 1.0f, out var h) || MathF.Abs(h.Point.Y - c.Y) > 0.15f || h.Normal.Y < 0.7f)
                    {
                        flat = false;
                        break;
                    }
                }

                if (!flat)
                {
                    Reject("floor not level");
                    continue;
                }

                // Room around (walls), the wall side may be at wallGap
                var room = true;
                var center = (wmin + wmax) / 2;
                foreach (var dir in new[] { Vector3.UnitX, -Vector3.UnitX, Vector3.UnitZ, -Vector3.UnitZ })
                {
                    if (wall && Vector3.Dot(dir, -wallN) > 0.7f)
                        continue;
                    var half = MathF.Abs(Vector3.Dot(wmax - center, dir));
                    foreach (var hy in new[] { 0.3f, MathF.Min(bmax.Y - bmin.Y, 1.5f) })
                    {
                        if (set.RayCast(new Vector3(center.X, c.Y + hy, center.Z), dir, half + clearance, out var h) && h.Distance < half + clearance - 1e-3f)
                            room = false;
                    }
                }

                if (!room)
                {
                    Reject("too close to the wall");
                    continue;
                }

                var inflate = new Vector3(0.02f, 0, 0.02f);
                if (obstacles.Any(b => BoxesOverlap(b.Min, b.Max, wmin + new Vector3(0, 0.05f, 0) + inflate, wmax - inflate)))
                {
                    Reject("obstacle");
                    continue;
                }

                if (plan.Any(p => new Vector2(p.Pos.X - pos.X, p.Pos.Z - pos.Z).Length() < MathF.Max(minDist, (p.Foot + foot) * 0.9f) ||
                                  BoxesOverlap(p.BMin, p.BMax, wmin + inflate, wmax - inflate)))
                {
                    Reject("spacing");
                    continue;
                }

                plan.Add((si, pos, q, PartRotation(src.Ent, q), scale, wmin, wmax, foot, wallPart));
            }

            // Names: number=N (<model>_<N+i>), suffix, else Smithbox's unique names
            var number = Has(args, "number") ? (int)GetFloat(args, "number", 0) : -1;
            var suffix = GetString(args, "suffix");
            var names = plan.Select((p, i) =>
            {
                var n = sources[p.Src].Name;
                if (number >= 0)
                {
                    var us = n.LastIndexOf('_');
                    return (us > 0 ? n[..us] : n) + "_" + (number + i).ToString("D4");
                }

                return suffix != null ? n + suffix + i : null;
            }).ToList();

            var taken = names.Where(n => n != null && target.GetObjectByName(n) != null).ToList();
            if (taken.Count > 0)
                throw new RemoteException($"{target.Name} already has: {string.Join(", ", taken.Take(10))}");

            result["placed"] = plan.Count;
            result["tries"] = tries - 1;
            result["rejected"] = new JsonObject(rejected.Select(r => KeyValuePair.Create(r.Key, (JsonNode)r.Value)));
            result["seed"] = seed;

            JsonArray Describe() => new(plan.Select((p, i) => (JsonNode)new JsonObject
            {
                ["src"] = $"{sources[p.Src].Map}/{sources[p.Src].Name}",
                ["name"] = names[i], ["pos"] = ToJson(p.Pos), ["rot"] = ToJson(p.Rot),
                ["scale"] = p.Scale != Vector3.One ? ToJson(p.Scale) : null, ["wall"] = p.Wall,
            }).ToArray());

            if (dry)
            {
                result["dry"] = true;
                result["parts"] = Describe();

                if (GetString(args, "path") is string png)
                {
                    var viewport = GetViewport(view);
                    var size = new int[2];
                    var capture = CaptureTo(view, viewport, png, args, (int)GetFloat(args, "settle", 6), size);
                    while (capture.MoveNext())
                        yield return null;

                    using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(png);
                    var sx = (float)image.Width / viewport.Width;
                    var sy = (float)image.Height / viewport.Height;
                    var used = new List<ViewportLabels.Box>();
                    var labels = new JsonArray();
                    var color = new Rgba32(255, 220, 40);

                    for (var i = 0; i < plan.Count; i++)
                    {
                        var p = plan[i];
                        // The box's outline on the floor and the name at its middle
                        var bottom = new[] { new Vector3(p.BMin.X, p.BMin.Y, p.BMin.Z), new Vector3(p.BMax.X, p.BMin.Y, p.BMin.Z),
                            new Vector3(p.BMax.X, p.BMin.Y, p.BMax.Z), new Vector3(p.BMin.X, p.BMin.Y, p.BMax.Z) };
                        bool OnScreen(Vector2 v, float margin) =>
                            v.X >= -margin * viewport.Width && v.Y >= -margin * viewport.Height &&
                            v.X <= (1 + margin) * viewport.Width && v.Y <= (1 + margin) * viewport.Height;

                        for (var k = 0; k < 4; k++)
                        {
                            var a = viewport.WorldToScreen(bottom[k], out var fa);
                            var b = viewport.WorldToScreen(bottom[(k + 1) % 4], out var fb);
                            if (fa && fb && OnScreen(a, 0.5f) && OnScreen(b, 0.5f))
                                ThickLine(image, a.X * sx, a.Y * sy, b.X * sx, b.Y * sy, color, 2);
                        }

                        var mid = viewport.WorldToScreen((p.BMin + p.BMax) / 2, out var front);
                        if (!front || !OnScreen(mid, 0))
                            continue;
                        var text = names[i] ?? $"{sources[p.Src].Name} #{i}";
                        ViewportLabels.Marker(image, (int)(mid.X * sx), (int)(mid.Y * sy), color);
                        Label(image, (int)(mid.X * sx), (int)(mid.Y * sy), text, color, used);
                        labels.Add(new JsonObject { ["name"] = text, ["x"] = (int)(mid.X * sx), ["y"] = (int)(mid.Y * sy) });
                    }

                    image.SaveAsPng(png);
                    result["path"] = png;
                    result["labels"] = labels;
                }

                yield break;
            }

            if (plan.Count == 0)
                throw new RemoteException("Nothing placed: no spot found (see rejected), widen the area or lower clearance/minDist");

            var clones = new List<MsbEntity>();
            var step = new RemoteStep($"scatter {plan.Count} parts", st =>
            {
                for (var i = 0; i < plan.Count; i++)
                {
                    var p = plan[i];
                    var src = sources[p.Src];
                    var clone = DuplicateInto(st, view, src.Ent, target, keepIds, names[i]);
                    st.Run(PartChange(clone, p.Pos, p.Rot, src.CanScale && p.Scale != PartScale(src.Ent) ? p.Scale : (Vector3?)null));
                    clones.Add(clone);
                    RecordAdd(st, clone, target.Name, src.Map, src.Name, keepIds, addAtlas);
                    if (p.Scale != Vector3.One)
                        st.Records[^1].Record["scale"] = ToJson(p.Scale);
                }
            });

            ExecuteStep(view, step, addAtlas);
            yield return null;

            result["parts"] = new JsonArray(clones.Select(c => (JsonNode)DescribePart(c)).ToArray());
            result["records"] = RecordsJson(step);
        }
        finally
        {
            if (!GetBool(args, "keepSource", false))
                foreach (var map in loadedHere)
                    view.Universe.UnloadMap(map);
        }
    }

    #endregion
}
