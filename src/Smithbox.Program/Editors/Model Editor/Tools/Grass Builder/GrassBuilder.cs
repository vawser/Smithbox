using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// What a new GRASS mesh is built from: the triangles to keep and the density each vertex starts with.
/// </summary>
/// <param name="MaxAngle">The steepest face kept, in degrees from straight up (+Y): 90 keeps walls, 180 keeps every
/// face, as vanilla files cover walls and overhangs too (GrassSurvey structure).</param>
/// <param name="Densities">The density of each of the six grass slots (the map piece part's GrassTypeParamID0-5).</param>
public sealed record GrassBuildOptions(float MaxAngle, float[] Densities);

/// <summary>
/// Builds Sekiro GRASS files by the rules of the 772 vanilla ones (GrassSurvey, Claude/Documentation/GRASS Builder.md):
/// <list type="bullet">
/// <item>Each face's normal is normalize(cross(C - A, B - A)). FLVER triangles are wound the same way (99.9% point
/// along their vertex normals), so copying a triangle keeps the side it faces.</item>
/// <item>The bounding volume hierarchy is a binary tree in preorder: StartChildIndex is the left child,
/// EndChildIndex the right, 0 for none (the root is never a child). A node's face range is its children's put
/// together, so the faces are sorted to match the tree.</item>
/// <item>Each volume's box fits the vertices of its faces exactly, and Unk10 is the mask of the density slots above
/// zero among them.</item>
/// </list>
/// Uses only SoulsFormats, so GrassSurvey's build check compiles this same file.
/// </summary>
public static class GrassBuilder
{
    public const int SlotCount = 6;

    /// <summary>
    /// The most faces a leaf holds before it is split. Vanilla leaves have a median of 32 faces (GrassSurvey);
    /// the game's own split rule isn't a plain one, so this only aims for leaves of that size.
    /// </summary>
    public const int LeafFaces = 32;

    /// <summary>The binder ID of a GRASS file: the FLVER's plus 1000 (772/772 vanilla mapbnds, 200 -> 1200).</summary>
    public const int BinderIdOffset = 1000;

    /// <summary>
    /// Adds a GRASS file for a model to its binder, as every vanilla holder does in SDT, ER, AC6 and NR (GrassSurvey
    /// buildcheck and later): {model}.grass in the FLVER's folder (mapbnd Model\, geombnd sib\), binder ID the FLVER's +
    /// 1000, Flag1, Zlib, kept in ID order. Null when the binder has no FLVER for the model, or already holds a GRASS file.
    /// </summary>
    public static BinderFile AddToBinder(IBinder binder, string modelName, byte[] data)
    {
        static string Stem(string name)
        {
            var file = name.Substring(name.LastIndexOf('\\') + 1);
            var dot = file.IndexOf('.');
            return dot < 0 ? file : file.Substring(0, dot);
        }

        if (binder.Files.Any(f => f.Name != null && f.Name.EndsWith(".grass", StringComparison.OrdinalIgnoreCase)))
            return null;

        var flver = binder.Files.FirstOrDefault(f => f.Name != null
            && f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)
            && string.Equals(Stem(f.Name), modelName, StringComparison.OrdinalIgnoreCase));

        if (flver == null)
            return null;

        var file = new BinderFile(Binder.FileFlags.Flag1, flver.ID + BinderIdOffset,
            flver.Name.Substring(0, flver.Name.Length - ".flver".Length) + ".grass", data)
        {
            CompressionType = DCX.Type.Zlib
        };

        var index = binder.Files.FindIndex(f => f.ID > file.ID);

        if (index < 0)
            binder.Files.Add(file);
        else
            binder.Files.Insert(index, file);

        return file;
    }

    /// <summary>Whether a face at this angle from up is kept.</summary>
    public static bool IsKept(Vector3 normal, float maxAngle)
    {
        if (maxAngle >= 180)
            return true;

        var angle = MathF.Acos(Math.Clamp(normal.Y, -1, 1)) * 180 / MathF.PI;

        return angle <= maxAngle + 1e-4f;
    }

    /// <summary>The normal a GRASS face of these points has: normalize(cross(C - A, B - A)), or zero when degenerate.</summary>
    public static Vector3 FaceNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        var n = Vector3.Cross(c - a, b - a);
        var length = n.Length();

        return length < 1e-9f ? Vector3.Zero : n / length;
    }

    /// <summary>
    /// The LOD 0 triangles of a FLVER mesh, in model space, as wound in the file.
    /// </summary>
    public static IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> GetMeshTriangles(FLVER2.Mesh mesh)
    {
        var faceSet = mesh.FaceSets.FirstOrDefault(f => f.Flags == FLVER2.FaceSet.FSFlags.None) ?? mesh.FaceSets.FirstOrDefault();

        if (faceSet == null)
            yield break;

        var indices = faceSet.Triangulate(mesh.Vertices.Count < ushort.MaxValue);

        for (var i = 0; i + 2 < indices.Count; i += 3)
        {
            int a = indices[i], b = indices[i + 1], c = indices[i + 2];

            if (a >= mesh.Vertices.Count || b >= mesh.Vertices.Count || c >= mesh.Vertices.Count)
                continue;

            yield return (mesh.Vertices[a].Position, mesh.Vertices[b].Position, mesh.Vertices[c].Position);
        }
    }

    /// <summary>How many of these triangles a build with this angle limit keeps.</summary>
    public static int CountKept(IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> triangles, float maxAngle)
    {
        var count = 0;

        foreach (var (a, b, c) in triangles)
        {
            var normal = FaceNormal(a, b, c);

            if (normal != Vector3.Zero && IsKept(normal, maxAngle))
                count++;
        }

        return count;
    }

    /// <summary>
    /// A GRASS mesh of the triangles that pass the angle limit, with vertices merged where they share a position
    /// (FLVER meshes split vertices along UV seams; vanilla GRASS files barely repeat a position, 1.2%) and every
    /// vertex given the chosen densities. Degenerate triangles are left out.
    /// </summary>
    public static GRASS Build(IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> triangles, GrassBuildOptions options)
    {
        var grass = new GRASS();
        var lookup = new Dictionary<Vector3, int>();

        int GetVertex(Vector3 position)
        {
            if (lookup.TryGetValue(position, out var index))
                return index;

            var vertex = new GRASS.Vertex { Position = position };

            for (var s = 0; s < SlotCount; s++)
            {
                vertex.GrassDensities[s] = options.Densities[s];
            }

            index = grass.Vertices.Count;
            grass.Vertices.Add(vertex);
            lookup[position] = index;

            return index;
        }

        foreach (var (a, b, c) in triangles)
        {
            var normal = FaceNormal(a, b, c);

            if (normal == Vector3.Zero || !IsKept(normal, options.MaxAngle))
                continue;

            var ia = GetVertex(a);
            var ib = GetVertex(b);
            var ic = GetVertex(c);

            // Points the same up to float precision
            if (ia == ib || ib == ic || ia == ic)
                continue;

            grass.Faces.Add(new GRASS.Face
            {
                VertexIndexA = ia,
                VertexIndexB = ib,
                VertexIndexC = ic,
                Normal = normal
            });
        }

        BuildHierarchy(grass);

        return grass;
    }

    /// <summary>
    /// Replaces the file's bounding volume hierarchy with one built over its faces, which it sorts to match the
    /// tree. Each leaf holds up to <see cref="LeafFaces"/> faces; a node is split at the median face centre along the
    /// longest side of its faces' centres.
    /// </summary>
    public static void BuildHierarchy(GRASS grass)
    {
        grass.BoundingVolumeHierarchy.Clear();

        if (grass.Faces.Count == 0)
            return;

        var faces = grass.Faces.ToArray();
        var centres = new Vector3[faces.Length];

        for (var i = 0; i < faces.Length; i++)
        {
            centres[i] = (grass.Vertices[faces[i].VertexIndexA].Position
                + grass.Vertices[faces[i].VertexIndexB].Position
                + grass.Vertices[faces[i].VertexIndexC].Position) / 3;
        }

        var order = Enumerable.Range(0, faces.Length).ToArray();
        var volumes = grass.BoundingVolumeHierarchy;

        // Iterative preorder, so a deep tree can't overflow the stack: each entry is a face range and the parent
        // field (left or right) to point at the volume made for it
        var stack = new Stack<(int Start, int End, int Parent, bool IsLeft)>();
        stack.Push((0, faces.Length, -1, false));

        while (stack.Count > 0)
        {
            var (start, end, parent, isLeft) = stack.Pop();
            var index = volumes.Count;
            var volume = new GRASS.Volume { StartFaceIndex = start, EndFaceIndex = end };
            volumes.Add(volume);

            if (parent >= 0)
            {
                if (isLeft)
                    volumes[parent].StartChildIndex = index;
                else
                    volumes[parent].EndChildIndex = index;
            }

            if (end - start <= LeafFaces)
                continue;

            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            for (var i = start; i < end; i++)
            {
                min = Vector3.Min(min, centres[order[i]]);
                max = Vector3.Max(max, centres[order[i]]);
            }

            var size = max - min;

            // Every face centre in the same place: no split separates them
            if (size.X <= 0 && size.Y <= 0 && size.Z <= 0)
                continue;

            var axis = size.X >= size.Y && size.X >= size.Z ? 0 : size.Y >= size.Z ? 1 : 2;

            Array.Sort(order, start, end - start, Comparer<int>.Create((x, y) =>
            {
                var cx = Axis(centres[x], axis);
                var cy = Axis(centres[y], axis);
                return cx != cy ? cx.CompareTo(cy) : x.CompareTo(y);
            }));

            var middle = start + (end - start) / 2;

            // Right pushed first, so the left child is made next (index + 1), as in every vanilla file
            stack.Push((middle, end, index, false));
            stack.Push((start, middle, index, true));
        }

        grass.Faces.Clear();

        foreach (var i in order)
        {
            grass.Faces.Add(faces[i]);
        }

        RefreshVolumes(grass);
    }

    /// <summary>
    /// Sets each volume's box to fit the vertices of its faces and Unk10 to the density slots above zero among them,
    /// as every vanilla volume has (15344/15344). Needed after vertices move or densities change.
    /// </summary>
    public static void RefreshVolumes(GRASS grass)
    {
        foreach (var volume in grass.BoundingVolumeHierarchy)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            var mask = 0;

            for (var f = volume.StartFaceIndex; f < volume.EndFaceIndex; f++)
            {
                var face = grass.Faces[f];

                foreach (var vi in new[] { face.VertexIndexA, face.VertexIndexB, face.VertexIndexC })
                {
                    var vertex = grass.Vertices[vi];

                    min = Vector3.Min(min, vertex.Position);
                    max = Vector3.Max(max, vertex.Position);

                    for (var s = 0; s < SlotCount; s++)
                    {
                        if (vertex.GrassDensities[s] > 0)
                            mask |= 1 << s;
                    }
                }
            }

            if (volume.EndFaceIndex <= volume.StartFaceIndex)
            {
                min = max = Vector3.Zero;
            }

            volume.BoundingBox = new GRASS.BoundingBox(min, max);
            volume.Unk10 = mask;
        }
    }

    private static float Axis(Vector3 v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

    /// <summary>
    /// The ways a GRASS file breaks the vanilla rules, empty when it follows them all: indices in range, the tree in
    /// preorder with each node's faces split between its children, every face in one leaf, tight boxes and exact
    /// masks, unit normals. For the survey's build check and the tool's own check before an action.
    /// </summary>
    public static List<string> Check(GRASS grass)
    {
        var problems = new List<string>();
        var volumes = grass.BoundingVolumeHierarchy;

        foreach (var face in grass.Faces)
        {
            if (face.VertexIndexA < 0 || face.VertexIndexA >= grass.Vertices.Count
                || face.VertexIndexB < 0 || face.VertexIndexB >= grass.Vertices.Count
                || face.VertexIndexC < 0 || face.VertexIndexC >= grass.Vertices.Count)
            {
                problems.Add("face vertex index out of range");
                return problems;
            }

            if (MathF.Abs(face.Normal.Length() - 1) > 1e-3f)
            {
                problems.Add("face normal not unit length");
                break;
            }
        }

        if (grass.Faces.Count == 0)
        {
            if (volumes.Count != 0)
                problems.Add("volumes without faces");
            return problems;
        }

        if (volumes.Count == 0)
        {
            problems.Add("faces without volumes");
            return problems;
        }

        var cover = new int[grass.Faces.Count];
        var visit = new List<int>();
        var stack = new Stack<int>();
        stack.Push(0);

        while (stack.Count > 0)
        {
            var i = stack.Pop();

            if (i < 0 || i >= volumes.Count || visit.Count > volumes.Count)
            {
                problems.Add("child index out of range or cycle");
                return problems;
            }

            visit.Add(i);
            var v = volumes[i];

            if (v.StartFaceIndex < 0 || v.EndFaceIndex > grass.Faces.Count || v.StartFaceIndex > v.EndFaceIndex)
            {
                problems.Add($"volume {i} face range out of bounds");
                return problems;
            }

            int left = v.StartChildIndex, right = v.EndChildIndex;

            if (left == 0 && right == 0)
            {
                for (var f = v.StartFaceIndex; f < v.EndFaceIndex; f++) cover[f]++;
                continue;
            }

            if (left != 0 && right != 0)
            {
                var l = volumes[left];
                var r = volumes[right];

                if (l.StartFaceIndex != v.StartFaceIndex || l.EndFaceIndex != r.StartFaceIndex || r.EndFaceIndex != v.EndFaceIndex)
                    problems.Add($"volume {i} faces not split between its children");
            }
            else
            {
                // One child: the node keeps the faces the child doesn't have (242 vanilla nodes)
                var c = volumes[left != 0 ? left : right];

                for (var f = v.StartFaceIndex; f < v.EndFaceIndex; f++)
                {
                    if (f < c.StartFaceIndex || f >= c.EndFaceIndex) cover[f]++;
                }
            }

            if (right != 0) stack.Push(right);
            if (left != 0) stack.Push(left);
        }

        if (!visit.SequenceEqual(Enumerable.Range(0, volumes.Count)))
            problems.Add("volumes not in preorder");

        if (cover.Any(c => c != 1))
            problems.Add("faces not each in exactly one leaf");

        var copy = new GRASS();
        copy.Vertices.AddRange(grass.Vertices);
        copy.Faces.AddRange(grass.Faces);

        foreach (var v in volumes)
        {
            copy.BoundingVolumeHierarchy.Add(new GRASS.Volume
            {
                StartFaceIndex = v.StartFaceIndex,
                EndFaceIndex = v.EndFaceIndex
            });
        }

        RefreshVolumes(copy);

        for (var i = 0; i < volumes.Count; i++)
        {
            var expected = copy.BoundingVolumeHierarchy[i];

            if (expected.Unk10 != volumes[i].Unk10)
            {
                problems.Add($"volume {i} Unk10 is not its density mask");
                break;
            }

            if (Vector3.Distance(expected.BoundingBox.Min, volumes[i].BoundingBox.Min) > 1e-4f
                || Vector3.Distance(expected.BoundingBox.Max, volumes[i].BoundingBox.Max) > 1e-4f)
            {
                problems.Add($"volume {i} box doesn't fit its faces");
                break;
            }
        }

        return problems;
    }
}
