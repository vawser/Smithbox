using Havok.Shared;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// One compressed mesh body of a collision file, decoded into welded vertices that can be moved.
/// Positions are in the space of the body's shape; <see cref="BodyTransform" /> places them in the part.
/// </summary>
public sealed class CollisionMakerBody
{
    /// <summary>Triangles per preview mesh, so moving a vertex only rebuilds the meshes near it.</summary>
    public const int ChunkTriangles = 4096;

    public readonly IHavokBody Body;
    public readonly Matrix4x4 BodyTransform;

    public readonly Vector3[] Vertices;
    public readonly Vector3[] OriginalVertices;
    public readonly int[] Indices;
    public readonly uint[] Materials;

    // The triangles of each vertex, as a flat list: VertexTriangles[VertexTriangleStart[v] .. VertexTriangleStart[v + 1]]
    private readonly int[] VertexTriangleStart;
    private readonly int[] VertexTriangles;

    private readonly bool[] DirtyChunks;
    private readonly MeshRenderableProxy[] ChunkMeshes;

    public BoundingBox Bounds { get; private set; }

    public int TriangleCount => Indices.Length / 3;

    public bool IsEdited { get; private set; }

    private CollisionMakerBody(IHavokBody body, List<Vector3> vertices, List<int> indices, List<uint> materials)
    {
        Body = body;
        BodyTransform = CollisionGeometry.BodyTransform(body);

        Vertices = vertices.ToArray();
        OriginalVertices = vertices.ToArray();
        Indices = indices.ToArray();
        Materials = materials.ToArray();

        // Count, then fill, the triangles of each vertex
        VertexTriangleStart = new int[Vertices.Length + 1];

        foreach (var index in Indices)
            VertexTriangleStart[index + 1]++;

        for (var v = 0; v < Vertices.Length; v++)
            VertexTriangleStart[v + 1] += VertexTriangleStart[v];

        VertexTriangles = new int[Indices.Length];
        var fill = (int[])VertexTriangleStart.Clone();

        for (var i = 0; i < Indices.Length; i++)
            VertexTriangles[fill[Indices[i]]++] = i / 3;

        var chunkCount = (TriangleCount + ChunkTriangles - 1) / ChunkTriangles;
        DirtyChunks = new bool[chunkCount];
        ChunkMeshes = new MeshRenderableProxy[chunkCount];

        UpdateBounds();
    }

    /// <summary>
    /// Decodes a body's compressed mesh. Corners that decode to the same position are welded into one vertex, as
    /// the vertices shared between sections are stored once. Each triangle keeps the collision material its shape key
    /// has in the FromSoftware parameters. Null if the body isn't a FromSoftware compressed mesh.
    /// </summary>
    public static CollisionMakerBody Decode(IHavokBody body)
    {
        if (body.Shape is not IHavokCompressedMesh mesh || !CollisionMakerShapes.IsSupportedShape(body.Shape))
            return null;

        var materialOfKey = CollisionMakerShapes.GetMaterialsByKey(body.Shape);
        var shift = 32 - mesh.BitsPerKey;

        var vertices = new List<Vector3>();
        var weld = new Dictionary<Vector3, int>();
        var indices = new List<int>();
        var materials = new List<uint>();

        int Add(Vector3 v)
        {
            if (!weld.TryGetValue(v, out var i))
            {
                i = vertices.Count;
                vertices.Add(v);
                weld[v] = i;
            }

            return i;
        }

        foreach (var (key, a, b, c) in CollisionGeometry.CompressedTriangles(mesh))
        {
            indices.Add(Add(a));
            indices.Add(Add(b));
            indices.Add(Add(c));

            materials.Add(materialOfKey.GetValueOrDefault(key >> shift));
        }

        if (indices.Count == 0)
            return null;

        return new CollisionMakerBody(body, vertices, indices, materials);
    }

    /// <summary>
    /// Finds the nearest triangle the ray hits, with the ray in the space of the shape.
    /// Returns the triangle and the distance along the ray.
    /// </summary>
    public bool RayCast(Ray ray, out int triangle, out float distance)
    {
        triangle = -1;
        distance = float.MaxValue;

        if (!ray.Intersects(Bounds))
            return false;

        for (var t = 0; t < TriangleCount; t++)
        {
            var a = Vertices[Indices[t * 3]];
            var b = Vertices[Indices[t * 3 + 1]];
            var c = Vertices[Indices[t * 3 + 2]];

            if (IntersectTriangle(ray, a, b, c, out var hit) && hit < distance)
            {
                distance = hit;
                triangle = t;
            }
        }

        return triangle >= 0;
    }

    /// <summary>The corner of a triangle nearest a point in the space of the shape.</summary>
    public int NearestCorner(int triangle, Vector3 point)
    {
        var best = Indices[triangle * 3];
        var bestDistance = Vector3.DistanceSquared(Vertices[best], point);

        for (var k = 1; k < 3; k++)
        {
            var v = Indices[triangle * 3 + k];
            var d = Vector3.DistanceSquared(Vertices[v], point);

            if (d < bestDistance)
            {
                best = v;
                bestDistance = d;
            }
        }

        return best;
    }

    /// <summary>
    /// The vertices within a radius of a point, with their weights: 1 at the point, falling smoothly to 0 at the
    /// radius. The target vertex is always included at full weight, even with no radius.
    /// </summary>
    public List<(int Vertex, float Weight)> GetFalloff(int target, float radius)
    {
        var result = new List<(int, float)> { (target, 1f) };

        if (radius <= 0)
            return result;

        var center = Vertices[target];
        var radiusSquared = radius * radius;

        for (var v = 0; v < Vertices.Length; v++)
        {
            if (v == target)
                continue;

            var d = Vector3.DistanceSquared(Vertices[v], center);

            if (d >= radiusSquared)
                continue;

            // Smoothstep from the edge of the radius in to the target
            var x = 1f - MathF.Sqrt(d) / radius;
            result.Add((v, x * x * (3f - 2f * x)));
        }

        return result;
    }

    /// <summary>Moves vertices by an offset in the space of the shape, scaled by their weights.</summary>
    public void Move(List<(int Vertex, float Weight)> vertices, Vector3 offset)
    {
        foreach (var (v, weight) in vertices)
        {
            Vertices[v] += offset * weight;
            MarkVertexDirty(v);
        }

        IsEdited = true;
        UpdateBounds();
    }

    /// <summary>Puts a vertex at a position in the space of the shape.</summary>
    public void SetVertex(int vertex, Vector3 position)
    {
        Vertices[vertex] = position;
        MarkVertexDirty(vertex);

        IsEdited = true;
        UpdateBounds();
    }

    /// <summary>Puts every vertex back where it was decoded.</summary>
    public void Reset()
    {
        Array.Copy(OriginalVertices, Vertices, Vertices.Length);
        Array.Fill(DirtyChunks, true);

        IsEdited = false;
        UpdateBounds();
    }

    private void MarkVertexDirty(int vertex)
    {
        for (var i = VertexTriangleStart[vertex]; i < VertexTriangleStart[vertex + 1]; i++)
        {
            DirtyChunks[VertexTriangles[i] / ChunkTriangles] = true;
        }
    }

    private void UpdateBounds()
    {
        Bounds = BoundingBox.CreateFromVertices(Vertices);
    }

    /// <summary>
    /// The triangles to rebuild the shape from: the edited vertices, and the triangles that still have three
    /// distinct corners with their materials.
    /// </summary>
    public (List<Vector3> Vertices, List<int> Indices, List<uint> Materials) GetBuildInput()
    {
        var indices = new List<int>(Indices.Length);
        var materials = new List<uint>(TriangleCount);

        for (var t = 0; t < TriangleCount; t++)
        {
            var a = Indices[t * 3];
            var b = Indices[t * 3 + 1];
            var c = Indices[t * 3 + 2];

            // Corners moved onto each other leave no area to collide with
            var normal = Vector3.Cross(Vertices[b] - Vertices[a], Vertices[c] - Vertices[a]);

            if (normal.LengthSquared() < 1e-12f)
                continue;

            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
            materials.Add(Materials[t]);
        }

        return (Vertices.ToList(), indices, materials);
    }

    #region Preview

    /// <summary>
    /// Builds the preview meshes, or rebuilds those with a moved vertex, and places them with the part.
    /// </summary>
    public void UpdatePreview(RenderScene scene, Matrix4x4 world, Color color, bool visible)
    {
        for (var chunk = 0; chunk < ChunkMeshes.Length; chunk++)
        {
            if (ChunkMeshes[chunk] == null || DirtyChunks[chunk])
            {
                ChunkMeshes[chunk]?.Dispose();
                ChunkMeshes[chunk] = BuildChunk(scene, chunk, color);
                DirtyChunks[chunk] = false;
            }

            var mesh = ChunkMeshes[chunk];
            var placement = BodyTransform * world;

            if (mesh.World != placement)
                mesh.World = placement;

            if (mesh.Visible != visible)
                mesh.Visible = visible;
        }
    }

    private MeshRenderableProxy BuildChunk(RenderScene scene, int chunk, Color color)
    {
        var first = chunk * ChunkTriangles;
        var count = Math.Min(ChunkTriangles, TriangleCount - first);

        var provider = RawMeshProvider.FromTriangles(Vertices,
            new ArraySegment<int>(Indices, first * 3, count * 3), color);

        var mesh = MeshRenderableProxy.FromProvider(scene, provider);
        mesh.DrawFilter = RenderFilter.Collision;

        return mesh;
    }

    public void DisposePreview()
    {
        for (var chunk = 0; chunk < ChunkMeshes.Length; chunk++)
        {
            ChunkMeshes[chunk]?.Dispose();
            ChunkMeshes[chunk] = null;
        }
    }

    #endregion

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
/// A collision file being edited: the compressed mesh bodies of one collision type of a collision part.
/// </summary>
public sealed class CollisionMakerFile
{
    public readonly MsbEntity Entity;
    public readonly string MapId;
    public readonly string FileName;
    public readonly HavokCollisionType Type;
    public readonly Havok.HavokDocument Document;
    public readonly List<CollisionMakerBody> Bodies;

    public CollisionMakerFile(MsbEntity entity, string mapId, string fileName, HavokCollisionType type,
        Havok.HavokDocument document, List<CollisionMakerBody> bodies)
    {
        Entity = entity;
        MapId = mapId;
        FileName = fileName;
        Type = type;
        Document = document;
        Bodies = bodies;
    }

    public bool IsEdited => Bodies.Any(b => b.IsEdited);

    /// <summary>
    /// The binder the file is in: h, l or f, from the first letter of its name.
    /// </summary>
    public string BinderType => FileName.Substring(0, 1).ToLowerInvariant();

    public void DisposePreview()
    {
        foreach (var body in Bodies)
            body.DisposePreview();
    }
}
