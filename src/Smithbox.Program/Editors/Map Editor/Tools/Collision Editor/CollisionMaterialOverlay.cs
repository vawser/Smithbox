using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Material tab's overlay: each collision triangle drawn over the collision mesh in the colour of its collision
/// material, translucent and lifted a little towards its front so it shows in front of the mesh, as the WALL cells
/// are. None of it can be picked, so clicks reach the collision behind it.
/// <para>
/// A body is drawn in chunks of <see cref="CollisionEditorBody.ChunkTriangles" /> triangles, each a batch per
/// material, as the debug primitives take a single colour. A chunk is rebuilt when a triangle in it is repainted (its
/// overlay version changes), and, as the Collision Editor's preview does, the rebuilt chunk is hidden until its
/// geometry has been uploaded, then drawn alongside the old one for <see cref="OverlapFrames" /> frames before the old
/// one goes, so painting never leaves gaps.
/// </para>
/// </summary>
public sealed class CollisionMaterialOverlay
{
    /// <summary>Frames a rebuilt chunk is drawn alongside the old one before the old one goes.</summary>
    public const int OverlapFrames = 2;

    // How far the overlay is lifted towards the front of each triangle: as far as the WALL cells, so it shows in
    // front of the collision mesh
    private const float SurfaceOffset = 0.02f;

    private sealed record Batch(DebugPrimitiveRenderableProxy Proxy, DbgPrimWallCells Primitive, uint Material);

    private sealed class Chunk
    {
        public CollisionEditorBody Body;
        public int Version = -1;
        public List<Batch> Shown = new();
        public List<Batch> Pending;
        public int PendingShownFrames;
    }

    // Keyed by file name, body index and chunk, so a file decoded again (after a commit) keeps its drawn chunks until
    // the rebuilt ones are drawn
    private readonly Dictionary<(string File, int Body, int Chunk), Chunk> Chunks = new();

    public bool IsShown => Chunks.Count > 0;

    /// <summary>
    /// Brings the overlay up to date: one entry per drawn body, with its placement and whether it is shown. Chunks of
    /// files not listed in <paramref name="wanted" /> go; files that are wanted but not yet decoded keep what they
    /// have drawn.
    /// </summary>
    public void Update(RenderScene scene, ISet<string> wanted,
        IReadOnlyList<(CollisionEditorFile File, bool Visible)> files, float opacity, ISet<uint> hidden)
    {
        Update(scene, wanted, files.Select(f => (f.File.FileName, (IReadOnlyList<CollisionEditorBody>)f.File.Bodies,
            f.File.Entity.GetWorldMatrix(), f.Visible)).ToList(), opacity, hidden);
    }

    /// <summary>
    /// As above, for collision that isn't a map part's (the Model Editor's): each file by a key of its own, its
    /// bodies, where it is drawn, and whether it is shown.
    /// </summary>
    public void Update(RenderScene scene, ISet<string> wanted,
        IReadOnlyList<(string Key, IReadOnlyList<CollisionEditorBody> Bodies, Matrix4x4 World, bool Visible)> files,
        float opacity, ISet<uint> hidden)
    {
        var seen = new HashSet<(string, int, int)>();
        var decoded = new HashSet<string>(files.Select(f => f.Key), StringComparer.OrdinalIgnoreCase);

        foreach (var (fileKey, bodies, world, visible) in files)
        {
            for (var b = 0; b < bodies.Count; b++)
            {
                var body = bodies[b];
                var placement = body.BodyTransform * world;

                for (var c = 0; c < body.ChunkCount; c++)
                {
                    var key = (fileKey, b, c);
                    seen.Add(key);

                    if (!Chunks.TryGetValue(key, out var chunk))
                    {
                        chunk = new Chunk();
                        Chunks[key] = chunk;
                    }

                    UpdateChunk(scene, chunk, body, c, placement, visible, opacity, hidden);
                }
            }
        }

        // Chunks past the end of a rebuilt body, and those of files no longer drawn
        foreach (var key in Chunks.Keys.ToList())
        {
            if (seen.Contains(key))
                continue;

            // Wanted but not decoded yet (after a commit drops it): kept as drawn, so the overlay doesn't leave a gap
            if (wanted.Contains(key.File) && !decoded.Contains(key.File))
                continue;

            Release(Chunks[key]);
            Chunks.Remove(key);
        }
    }

    private static void UpdateChunk(RenderScene scene, Chunk chunk, CollisionEditorBody body, int index,
        Matrix4x4 placement, bool visible, float opacity, ISet<uint> hidden)
    {
        // Rebuilt when repainted, or for a body decoded again; a rebuild still on its way finishes first
        if (chunk.Pending == null && (chunk.Body != body || chunk.Version != body.GetOverlayVersion(index)))
        {
            chunk.Body = body;
            chunk.Version = body.GetOverlayVersion(index);
            chunk.Pending = Build(scene, body, index, opacity);
            chunk.PendingShownFrames = 0;
        }

        if (chunk.Pending != null)
        {
            if (chunk.PendingShownFrames >= OverlapFrames)
            {
                // Drawn alongside the old batches for long enough: they take over
                ReleaseBatches(chunk.Shown);
                chunk.Shown = chunk.Pending;
                chunk.Pending = null;
                chunk.PendingShownFrames = 0;
            }
            else if (chunk.PendingShownFrames > 0 || chunk.Pending.All(p => p.Proxy.IsRenderableConstructed))
            {
                Place(chunk.Pending, placement, visible, opacity, hidden);
                chunk.PendingShownFrames++;
            }
            else
            {
                // Still uploading, so hidden until it can be drawn whole
                foreach (var batch in chunk.Pending)
                    batch.Proxy.World = placement;
            }
        }

        Place(chunk.Shown, placement, visible, opacity, hidden);
    }

    private static void Place(List<Batch> batches, Matrix4x4 placement, bool visible, float opacity, ISet<uint> hidden)
    {
        foreach (var batch in batches)
        {
            if (batch.Proxy.World != placement)
                batch.Proxy.World = placement;

            var shown = visible && !hidden.Contains(batch.Material);

            if (batch.Proxy.Visible != shown)
                batch.Proxy.Visible = shown;

            // The opacity follows the option as it is changed
            var fill = ColorHelper.GetTransparencyColor(GetColor(batch.Material), opacity);

            if (batch.Proxy.BaseColor != fill)
            {
                batch.Proxy.BaseColor = fill;
                batch.Proxy.HighlightedColor = fill;
            }
        }
    }

    /// <summary>The batches of a chunk, one per material, hidden until they have been uploaded.</summary>
    private static List<Batch> Build(RenderScene scene, CollisionEditorBody body, int index, float opacity)
    {
        var first = index * CollisionEditorBody.ChunkTriangles;
        var count = Math.Min(CollisionEditorBody.ChunkTriangles, body.TriangleCount - first);
        var byMaterial = new Dictionary<uint, List<(Vector3, Vector3, Vector3)>>();

        for (var t = first; t < first + count; t++)
        {
            var (a, b, c) = body.GetTriangle(t);
            var normal = Vector3.Cross(b - a, c - a);

            // Corners moved onto each other leave nothing to draw
            if (normal.LengthSquared() < 1e-12f)
                continue;

            var offset = Vector3.Normalize(normal) * SurfaceOffset;

            if (!byMaterial.TryGetValue(body.Materials[t], out var list))
            {
                list = new List<(Vector3, Vector3, Vector3)>();
                byMaterial[body.Materials[t]] = list;
            }

            list.Add((a + offset, b + offset, c + offset));
        }

        var batches = new List<Batch>();

        foreach (var (material, triangles) in byMaterial)
        {
            var fill = ColorHelper.GetTransparencyColor(GetColor(material), opacity);

            foreach (var part in triangles.Chunk(DbgPrimWallCells.MaxTriangles))
            {
                var primitive = new DbgPrimWallCells(part);
                var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
                {
                    Pickable = false,
                    RenderTranslucent = true,
                    BaseColor = fill,
                    HighlightedColor = fill,
                    // Hidden with the collision it lies on
                    DrawFilter = RenderFilter.Collision,
                    Visible = false,
                };

                batches.Add(new Batch(proxy, primitive, material));
            }
        }

        return batches;
    }

    /// <summary>Removes the whole overlay at once.</summary>
    public void Clear()
    {
        foreach (var chunk in Chunks.Values)
            Release(chunk);

        Chunks.Clear();
    }

    private static void Release(Chunk chunk)
    {
        ReleaseBatches(chunk.Shown);

        if (chunk.Pending != null)
            ReleaseBatches(chunk.Pending);

        chunk.Pending = null;
    }

    private static void ReleaseBatches(List<Batch> batches)
    {
        foreach (var batch in batches)
        {
            batch.Proxy.Dispose();
            batch.Primitive.Release();
        }

        batches.Clear();
    }

    /// <summary>
    /// A material's colour (0 to 1), the same for the same material ID wherever it is drawn. The surface (the
    /// HitMtrlParam row, the ID's last two digits) picks the hue, spread by the golden ratio so neighbouring rows
    /// differ; the property in the hundreds (Safe, Slide) shifts its saturation and brightness, so 2 and 402 are told
    /// apart but still read as related.
    /// </summary>
    public static Vector3 GetColor(uint material)
    {
        var surface = material % 100;
        var property = (material / 100) % 6;

        var hue = (0.08f + surface * 0.6180339887f) % 1f;
        var saturation = property == 0 ? 0.75f : 0.45f + 0.08f * (property % 3);
        var value = property == 0 ? 0.95f : 0.70f + 0.1f * (property % 2);

        return HsvToRgb(hue, saturation, value);
    }

    private static Vector3 HsvToRgb(float h, float s, float v)
    {
        var i = (int)MathF.Floor(h * 6f);
        var f = h * 6f - i;
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);

        return (i % 6) switch
        {
            0 => new Vector3(v, t, p),
            1 => new Vector3(q, v, p),
            2 => new Vector3(p, v, t),
            3 => new Vector3(p, q, v),
            4 => new Vector3(t, p, v),
            _ => new Vector3(v, p, q),
        };
    }
}
