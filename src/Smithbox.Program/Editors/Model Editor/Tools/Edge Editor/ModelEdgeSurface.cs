using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The drawn triangles of a model's FLVER in model space, for placing and dragging EDGE points over it in the Model
/// Editor's EDGE Editor. Rigid meshes are placed by their bones, as the viewport draws them (see
/// <see cref="MeshTransformAction.GetDrawnCenter"/>). Only the full-detail face set of each mesh is used.
/// <para>
/// Object EDGE points lie on the FLVER (EdgeSurvey objects: 220 of 235 grapple points within 20 cm of it); ledges and
/// walls less closely (median 10-30 cm), as the game places those on the object's collision.
/// </para>
/// </summary>
public sealed class ModelEdgeSurface
{
    private sealed class Chunk
    {
        public BoundingBox Bounds;
        public readonly List<Vector3> Corners = new();
    }

    private readonly List<Chunk> Chunks = new();

    // Triangles per chunk, so a ray tests only the chunks it passes through
    private const int ChunkTriangles = 512;

    public int TriangleCount { get; private set; }

    public static ModelEdgeSurface Build(FLVER2 flver, ProjectType projectType)
    {
        var surface = new ModelEdgeSurface();

        if (flver == null)
            return surface;

        var boneWorlds = flver.Nodes.Select(n => Utils.GetBoneWorldMatrix(n, flver.Nodes)).ToArray();

        foreach (var mesh in flver.Meshes)
        {
            if (mesh.Vertices.Count == 0)
                continue;

            var faceSet = mesh.FaceSets.FirstOrDefault(f => f.Flags == FLVER2.FaceSet.FSFlags.None)
                ?? mesh.FaceSets.FirstOrDefault();

            if (faceSet == null)
                continue;

            var placement = new MeshTransformAction.VertexPlacement(flver, mesh, projectType);
            var drawn = new Vector3[mesh.Vertices.Count];

            for (var i = 0; i < drawn.Length; i++)
            {
                var vertex = mesh.Vertices[i];
                var node = placement.GetNode(vertex);

                drawn[i] = node >= 0 && node < boneWorlds.Length
                    ? Vector3.Transform(vertex.Position, boneWorlds[node])
                    : vertex.Position;
            }

            List<int> indices;

            try
            {
                indices = faceSet.Triangulate(mesh.Vertices.Count < ushort.MaxValue);
            }
            catch
            {
                continue;
            }

            Chunk chunk = null;

            for (var i = 0; i + 2 < indices.Count; i += 3)
            {
                int a = indices[i], b = indices[i + 1], c = indices[i + 2];

                if (a < 0 || b < 0 || c < 0 || a >= drawn.Length || b >= drawn.Length || c >= drawn.Length)
                    continue;

                if (chunk == null || chunk.Corners.Count >= ChunkTriangles * 3)
                {
                    chunk = new Chunk { Bounds = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue)) };
                    surface.Chunks.Add(chunk);
                }

                chunk.Corners.Add(drawn[a]);
                chunk.Corners.Add(drawn[b]);
                chunk.Corners.Add(drawn[c]);

                var min = Vector3.Min(chunk.Bounds.Min, Vector3.Min(drawn[a], Vector3.Min(drawn[b], drawn[c])));
                var max = Vector3.Max(chunk.Bounds.Max, Vector3.Max(drawn[a], Vector3.Max(drawn[b], drawn[c])));
                chunk.Bounds = new BoundingBox(min, max);

                surface.TriangleCount++;
            }
        }

        return surface;
    }

    /// <summary>The nearest hit of a ray (unit direction, model space), either side of the triangles.</summary>
    public bool RayCast(Vector3 origin, Vector3 direction, out float distance)
    {
        distance = float.MaxValue;
        var ray = new Ray(origin, direction);

        foreach (var chunk in Chunks)
        {
            if (!ray.Intersects(chunk.Bounds))
                continue;

            for (var i = 0; i + 2 < chunk.Corners.Count; i += 3)
            {
                if (RayTriangle(origin, direction, chunk.Corners[i], chunk.Corners[i + 1], chunk.Corners[i + 2]) is float t
                    && t < distance)
                {
                    distance = t;
                }
            }
        }

        return distance < float.MaxValue;
    }

    // Möller–Trumbore, both faces
    private static float? RayTriangle(Vector3 origin, Vector3 direction, Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = b - a;
        var ac = c - a;
        var p = Vector3.Cross(direction, ac);
        var det = Vector3.Dot(ab, p);

        if (MathF.Abs(det) < 1e-12f)
            return null;

        var inv = 1.0f / det;
        var s = origin - a;
        var u = Vector3.Dot(s, p) * inv;

        if (u < 0 || u > 1)
            return null;

        var q = Vector3.Cross(s, ab);
        var v = Vector3.Dot(direction, q) * inv;

        if (v < 0 || u + v > 1)
            return null;

        var t = Vector3.Dot(ac, q) * inv;

        return t > 1e-5f ? t : null;
    }
}
