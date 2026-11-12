using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The vertices of one room's NVM being edited (NVM mode's Edit tab): a working copy of its positions in the part's
/// space, placed by the part. The NVM isn't changed until the edit is committed, which builds a copy with the moved
/// vertices (NvmEdit.WithVertices: topology kept, root box and box tree rebuilt the game's way). The falloff and moves
/// are those of the Havok mode's NavVertexFile. Renderer-free, so NavSurvey links it.
/// </summary>
public sealed class NvmVertexCopy
{
    public NvmRoom Room { get; }

    public NVM Nvm => Room.Nvm;

    public Vector3[] Original { get; }

    public Vector3[] Vertices { get; }

    public Matrix4x4 World { get; private set; }
    private Matrix4x4 Inverse;

    /// <summary>Bumped by every move, so the preview knows when to be drawn again.</summary>
    public int Revision { get; private set; }

    private readonly HashSet<int> Touched = new();

    public NvmVertexCopy(NvmRoom room)
    {
        Room = room;
        Original = room.Nvm.Vertices.ToArray();
        Vertices = (Vector3[])Original.Clone();
        World = room.World;
        if (!Matrix4x4.Invert(World, out Inverse))
            Inverse = Matrix4x4.Identity;
    }

    public bool IsEdited => Touched.Any(i => Vertices[i] != Original[i]);

    public int MovedCount => Touched.Count(i => Vertices[i] != Original[i]);

    public Vector3 WorldVertex(int index) => Vector3.Transform(Vertices[index], World);

    /// <summary>The nearest triangle under a ray (map space), from the edited positions.</summary>
    public bool RayCast(Vector3 origin, Vector3 direction, out int triangle, out float distance, out Vector3 point)
    {
        triangle = -1;
        distance = float.MaxValue;
        point = default;

        var localOrigin = Vector3.Transform(origin, Inverse);
        var localDirection = Vector3.TransformNormal(direction, Inverse);

        for (int t = 0; t < Nvm.Triangles.Count; t++)
        {
            var tri = Nvm.Triangles[t];
            if (tri.VertexIndex1 >= Vertices.Length || tri.VertexIndex2 >= Vertices.Length || tri.VertexIndex3 >= Vertices.Length)
                continue;

            if (!NavigationOverlayBuilder.RayTriangle(localOrigin, localDirection, Vertices[tri.VertexIndex1], Vertices[tri.VertexIndex2], Vertices[tri.VertexIndex3], out float hit))
                continue;

            var p = Vector3.Transform(localOrigin + localDirection * hit, World);
            float d = Vector3.Distance(origin, p);

            if (d < distance)
            {
                distance = d;
                triangle = t;
                point = p;
            }
        }

        return triangle >= 0;
    }

    public int NearestCorner(int triangle, Vector3 worldPoint)
    {
        var tri = Nvm.Triangles[triangle];
        return NvmGeometry.VertexIndices(tri).OrderBy(v => Vector3.DistanceSquared(WorldVertex(v), worldPoint)).First();
    }

    /// <summary>
    /// The vertices within the inner distance of a point (fully), then fading smoothly out over the radius, in map units.
    /// </summary>
    public List<(int Vertex, float Weight)> Falloff(Vector3 worldPoint, float inner, float radius)
    {
        var result = new List<(int, float)>();
        radius = Math.Max(0f, radius);
        inner = Math.Max(0f, inner);
        float reach = inner + radius;

        for (int v = 0; v < Vertices.Length; v++)
        {
            float d = Vector3.Distance(WorldVertex(v), worldPoint);

            if (d <= inner)
            {
                result.Add((v, 1f));
            }
            else if (radius > 0 && d < reach)
            {
                float x = 1f - (d - inner) / radius;
                result.Add((v, x * x * (3f - 2f * x)));
            }
        }

        return result;
    }

    public void MoveWorld(List<(int Vertex, float Weight)> vertices, Vector3 worldOffset)
    {
        if (vertices.Count == 0)
            return;

        var local = Vector3.TransformNormal(worldOffset, Inverse);

        foreach (var (v, weight) in vertices)
        {
            Vertices[v] += local * weight;
            Touched.Add(v);
        }

        Revision++;
    }

    public void OffsetWorld(List<(int Vertex, Vector3 Offset)> offsets)
    {
        if (offsets.Count == 0)
            return;

        foreach (var (v, offset) in offsets)
        {
            Vertices[v] += Vector3.TransformNormal(offset, Inverse);
            Touched.Add(v);
        }

        Revision++;
    }

    public void Reset()
    {
        Array.Copy(Original, Vertices, Original.Length);
        Touched.Clear();
        Revision++;
    }
}
