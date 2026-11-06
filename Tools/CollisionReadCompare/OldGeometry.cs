using System.Numerics;
using SoulsFormats;

// Copies of the geometry part of Smithbox's collision and navmesh code before Phase 6.2 (Editors/Map Editor/Utils/
// HKX_Helper, HKX2_Helper, HKLib_Helper, HKLib2019_Helper and Renderer/Resource Types/HavokNavmeshResource), without the
// GPU upload. Each returns triangle lists (three vertices a triangle), one per submesh. Kept faithful, bugs included.

public static class OldSoulsFormats
{
    public static HKX.HKXVariation Variation(string game) => game == "BB" ? HKX.HKXVariation.HKXBloodBorne : HKX.HKXVariation.HKXDS3;

    /// <summary>HKX_Helper.LoadCollisionMesh: <paramref name="fixedBB" /> processes each compressed mesh once, with its body.</summary>
    public static List<List<Vector3>> Collision(HKX hkx, bool fixedBB = false)
    {
        var result = new List<List<Vector3>>();
        foreach (HKX.HKXObject obj in hkx.DataSection.Objects)
        {
            if (obj is HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage col)
                result.Add(Storage(col));

            if (obj is HKX.FSNPCustomParamCompressedMeshShape ncol)
            {
                HKX.HKNPBodyCInfo bodyInfo = null;
                if (fixedBB)
                {
                    foreach (var sys in hkx.DataSection.Objects.OfType<HKX.HKNPPhysicsSystemData>())
                        foreach (HKX.HKNPBodyCInfo info in sys.Bodies.GetArrayData().Elements)
                            if (info.ShapeReference.DestObject == ncol) bodyInfo ??= info;
                    result.Add(Compressed(ncol, bodyInfo));
                    continue;
                }

                // as written: a mesh per object before the physics system data, without a body
                foreach (HKX.HKXObject scene in hkx.DataSection.Objects)
                {
                    if (scene is HKX.HKNPPhysicsSystemData sys)
                    {
                        foreach (HKX.HKNPBodyCInfo info in sys.Bodies.GetArrayData().Elements)
                            if (info.ShapeReference.DestObject == ncol) { bodyInfo = info; break; }
                        break;
                    }
                    try { result.Add(Compressed(ncol, bodyInfo)); } catch { }
                }
            }
        }
        return result.Where(r => r.Count > 0).ToList();
    }

    private static List<Vector3> Storage(HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage mesh)
    {
        dynamic indices;
        if (mesh.Indices8?.Capacity > 0) indices = mesh.Indices8.GetArrayData().Elements;
        else if (mesh.Indices16?.Capacity > 0) indices = mesh.Indices16.GetArrayData().Elements;
        else indices = mesh.Indices32.GetArrayData().Elements;

        var tris = new List<Vector3>();
        for (var id = 0; id < indices.Count; id += 4)
        {
            for (int k = 0; k < 3; k++)
            {
                Vector4 v = mesh.Vertices[(int)indices[id + k].data].Vector;
                tris.Add(new Vector3(v.X, v.Y, v.Z));
            }
        }
        return tris;
    }

    private static List<Vector3> Compressed(HKX.FSNPCustomParamCompressedMeshShape mesh, HKX.HKNPBodyCInfo body)
    {
        var verts = new List<Vector3>();
        HKX.HKNPCompressedMeshShapeData coldata = mesh.GetMeshShapeData();
        Vector3 Get(HKX.CollisionMeshChunk section, byte idx)
        {
            if (idx < section.sharedVerticesLength)
                return Transform(coldata.packedVertices.GetArrayData().Elements[(ushort)(idx + section.firstPackedVertex)]
                    .Decompress(section.SmallVertexScale, section.SmallVertexOffset), body);
            var index = coldata.sharedVerticesIndex.GetArrayData().Elements[idx + section.sharedVerticesIndex - section.sharedVerticesLength].data;
            return Transform(coldata.sharedVertices.GetArrayData().Elements[index].Decompress(coldata.BoundingBoxMin, coldata.BoundingBoxMax), body);
        }
        foreach (HKX.CollisionMeshChunk section in coldata.sections.GetArrayData().Elements)
        {
            for (var i = 0; i < section.primitivesLength; i++)
            {
                HKX.MeshPrimitive tri = coldata.primitives.GetArrayData().Elements[i + section.primitivesIndex];
                if (tri.Idx0 == 0xDE && tri.Idx1 == 0xAD && tri.Idx2 == 0xDE && tri.Idx3 == 0xAD) continue;
                verts.Add(Get(section, tri.Idx0)); verts.Add(Get(section, tri.Idx1)); verts.Add(Get(section, tri.Idx2));
                if (tri.Idx2 != tri.Idx3)
                {
                    verts.Add(verts[^3]); verts.Add(verts[^2]); verts.Add(Get(section, tri.Idx3));
                }
            }
        }
        return verts;
    }

    private static Vector3 Transform(Vector3 vert, HKX.HKNPBodyCInfo body)
    {
        if (body == null) return vert;
        Vector3 trans = new(body.Position.Vector.X, body.Position.Vector.Y, body.Position.Vector.Z);
        Quaternion quat = new(body.Orientation.Vector.X, body.Orientation.Vector.Y, body.Orientation.Vector.Z, body.Orientation.Vector.W);
        return Vector3.Transform(vert, quat) + trans;
    }
}

public static class OldHkx2
{
    /// <summary>HKX2_Helper.LoadCollisionMesh.</summary>
    public static List<List<Vector3>> Collision(HKX2.hkRootLevelContainer root)
    {
        var result = new List<List<Vector3>>();
        if (root.m_namedVariants.Count == 0) return result;
        var scene = root.m_namedVariants.Select(v => v.m_variant).OfType<HKX2.hknpPhysicsSceneData>().FirstOrDefault();
        if (scene == null) return result;

        foreach (HKX2.hknpBodyCinfo body in scene.m_systemDatas.SelectMany(s => s.m_bodyCinfos))
        {
            try
            {
                List<Vector3> verts; List<int> indices;
                if (body.m_shape is HKX2.fsnpCustomParamCompressedMeshShape ncol) (verts, indices) = Compressed(ncol, body);
                else if (body.m_shape is HKX2.hknpExternMeshShape ext) (verts, indices) = Extern(ext, body);
                else if (body.m_shape is HKX2.hknpConvexPolytopeShape poly) (verts, indices) = Polytope(poly, body);
                else continue;
                result.Add(indices.Select(i => verts[i]).ToList());
            }
            catch { }
        }
        return result.Where(r => r.Count > 0).ToList();
    }

    private static (List<Vector3>, List<int>) Compressed(HKX2.fsnpCustomParamCompressedMeshShape mesh, HKX2.hknpBodyCinfo body)
    {
        var verts = new List<Vector3>();
        var indices = new List<int>();
        HKX2.hknpCompressedMeshShapeData coldata = mesh.m_data;
        foreach (HKX2.hkcdStaticMeshTreeBaseSection section in coldata.m_meshTree.m_sections)
        {
            var sharedVerticesLength = section.m_sharedVertices.m_data & 0xFF;
            var sharedVerticesIndex = section.m_sharedVertices.m_data >> 8;
            var offset = new Vector3(section.m_codecParms_0, section.m_codecParms_1, section.m_codecParms_2);
            var scale = new Vector3(section.m_codecParms_3, section.m_codecParms_4, section.m_codecParms_5);
            void Add(byte idx)
            {
                indices.Add(verts.Count);
                if (idx < sharedVerticesLength)
                    verts.Add(Transform(coldata.DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(ushort)(idx + section.m_firstPackedVertex)], scale, offset), body));
                else
                    verts.Add(Transform(coldata.DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[
                        coldata.m_meshTree.m_sharedVerticesIndex[(int)(idx + sharedVerticesIndex - sharedVerticesLength)]],
                        coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max), body));
            }
            for (var i = 0; i < (section.m_primitives.m_data & 0xFF); i++)
            {
                var tri = coldata.m_meshTree.m_primitives[i + (int)(section.m_primitives.m_data >> 8)];
                if (tri.m_indices_0 == 0xDE && tri.m_indices_1 == 0xAD && tri.m_indices_2 == 0xDE && tri.m_indices_3 == 0xAD) continue;
                Add(tri.m_indices_0); Add(tri.m_indices_1); Add(tri.m_indices_2);
                if (tri.m_indices_2 != tri.m_indices_3)
                {
                    indices.Add(verts.Count); verts.Add(verts[^3]);
                    indices.Add(verts.Count); verts.Add(verts[^2]);
                    Add(tri.m_indices_3);
                }
            }
        }
        return (verts, indices);
    }

    private static (List<Vector3>, List<int>) Extern(HKX2.hknpExternMeshShape shape, HKX2.hknpBodyCinfo body)
    {
        var verts = new List<Vector3>(); var indices = new List<int>();
        if (shape.m_geometry is HKX2.hknpDefaultExternMeshShapeGeometry g && g.m_geometry is HKX2.hkGeometry geo)
        {
            verts = geo.m_vertices.Select(v => Transform(new Vector3(v.X, v.Y, v.Z), body)).ToList();
            foreach (var t in geo.m_triangles) { indices.Add(t.m_a); indices.Add(t.m_b); indices.Add(t.m_c); }
        }
        return (verts, indices);
    }

    private static (List<Vector3>, List<int>) Polytope(HKX2.hknpConvexPolytopeShape shape, HKX2.hknpBodyCinfo body)
    {
        var verts = shape.m_vertices.Select(v => Transform(new Vector3(v.X, v.Y, v.Z), body)).ToList();
        var indices = new List<int>();
        foreach (var face in shape.m_faces)
            for (var i = 1; i < face.m_numIndices - 1; i++)
            {
                indices.Add(shape.m_indices[face.m_firstIndex]);
                indices.Add(shape.m_indices[face.m_firstIndex + i]);
                indices.Add(shape.m_indices[face.m_firstIndex + i + 1]);
            }
        return (verts, indices);
    }

    private static Vector3 Transform(Vector3 vert, HKX2.hknpBodyCinfo body) =>
        body == null ? vert : Vector3.Transform(vert, body.m_orientation) + new Vector3(body.m_position.X, body.m_position.Y, body.m_position.Z);

    /// <summary>HavokNavmeshResource.ProcessMesh_HKX2 and ProcessGraph_HKX2.</summary>
    public static (List<Vector3> Mesh, List<Vector3> Graph) Navmesh(HKX2.hkRootLevelContainer root)
    {
        var mesh = root.FindVariant<HKX2.hkaiNavMesh>();
        var graph = root.FindVariant<HKX2.hkaiDirectedGraphExplicitCost>();
        return (mesh == null ? null : OldNavmesh.Mesh(mesh.m_vertices, mesh.m_faces.Select(f => (f.m_startEdgeIndex, (int)f.m_numEdges)).ToList(), mesh.m_edges.Select(e => e.m_a).ToList()),
                graph == null ? null : OldNavmesh.Graph(graph.m_positions, graph.m_nodes.Select(n => (n.m_startEdgeIndex, n.m_numEdges)).ToList(), graph.m_edges.Select(e => (int)e.m_target).ToList()));
    }
}

public static class OldHkLib2018
{
    /// <summary>HKLib_Helper.LoadCollisionMesh.</summary>
    public static List<List<Vector3>> Collision(HKLib.hk2018.hkRootLevelContainer root)
    {
        var result = new List<List<Vector3>>();
        if (root.m_namedVariants.Count == 0) return result;
        var scene = root.m_namedVariants.Select(v => v.m_variant).FirstOrDefault(v => v is HKLib.hk2018.hknpPhysicsSceneData) ?? root.m_namedVariants[0].m_variant;
        if (scene is not HKLib.hk2018.hknpPhysicsSceneData physicsscene) return result;

        foreach (HKLib.hk2018.hknpBodyCinfo body in physicsscene.m_systemDatas.SelectMany(s => s.m_bodyCinfos))
        {
            try
            {
                List<Vector3> verts; List<int> indices;
                if (body.m_shape is HKLib.hk2018.hknpCompressedMeshShape ncol) (verts, indices) = Compressed((HKLib.hk2018.hknpCompressedMeshShapeData)ncol.m_data, body);
                else if (body.m_shape is HKLib.hk2018.hknpExternMeshShape ext)
                {
                    if (ext.m_geometry is not HKLib.hk2018.hknpDefaultExternMeshShapeGeometry { m_geometry: HKLib.hk2018.hkGeometry geo }) continue;
                    // as written: extern meshes aren't moved by their body
                    verts = geo.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
                    indices = geo.m_triangles.SelectMany(t => new[] { t.m_a, t.m_b, t.m_c }).ToList();
                }
                else if (body.m_shape is HKLib.hk2018.hknpConvexPolytopeShape poly)
                {
                    verts = poly.m_vertices.Select(v => Transform(new Vector3(v.X, v.Y, v.Z), body)).ToList();
                    indices = new List<int>();
                    foreach (var face in poly.m_faces)
                        for (var i = 1; i < face.m_numIndices - 1; i++)
                        {
                            indices.Add(poly.m_indices[face.m_firstIndex]);
                            indices.Add(poly.m_indices[face.m_firstIndex + i]);
                            indices.Add(poly.m_indices[face.m_firstIndex + i + 1]);
                        }
                }
                else continue;
                if (indices.Count > 0) result.Add(indices.Select(i => verts[i]).ToList());
            }
            catch { }
        }
        return result;
    }

    private static (List<Vector3>, List<int>) Compressed(HKLib.hk2018.hknpCompressedMeshShapeData coldata, HKLib.hk2018.hknpBodyCinfo body)
    {
        var verts = new List<Vector3>(); var ints = new List<int>();
        var tree = coldata.m_meshTree;
        if (tree.m_packedVertices.Count == 0 && tree.m_sharedVertices.Count == 0) return (verts, ints);
        foreach (var section in tree.m_sections)
        {
            for (int p = 0; p < section.m_numPrimitives; ++p)
            {
                var primitive = tree.m_primitives[p + (int)section.m_firstPrimitiveIndex];
                if (primitive.m_indices[0] == 222 && primitive.m_indices[1] == 173 && primitive.m_indices[2] == 222 && primitive.m_indices[3] == 173) continue;
                var offset = new Vector3(section.m_codecParms[0], section.m_codecParms[1], section.m_codecParms[2]);
                var scale = new Vector3(section.m_codecParms[3], section.m_codecParms[4], section.m_codecParms[5]);
                void Add(byte idx)
                {
                    ints.Add(verts.Count);
                    if (idx < section.m_numPackedVertices)
                        verts.Add(Transform(Packed(tree.m_packedVertices[(int)(idx + section.m_firstPackedVertexIndex)], scale, offset), body));
                    else
                        verts.Add(Transform(Shared(tree.m_sharedVertices[tree.m_sharedVerticesIndex[idx + (int)section.m_firstSharedVertexIndex - section.m_numPackedVertices]],
                            tree.m_domain.m_min, tree.m_domain.m_max), body));
                }
                Add(primitive.m_indices[0]); Add(primitive.m_indices[1]); Add(primitive.m_indices[2]);
                if (primitive.m_indices[2] != primitive.m_indices[3])
                {
                    ints.Add(verts.Count); verts.Add(verts[^3]);
                    ints.Add(verts.Count); verts.Add(verts[^2]);
                    Add(primitive.m_indices[3]);
                }
            }
        }
        return (verts, ints);
    }

    private static Vector3 Transform(Vector3 v, HKLib.hk2018.hknpBodyCinfo body) =>
        body == null ? v : Vector3.Transform(v, body.m_orientation) + new Vector3(body.m_position.X, body.m_position.Y, body.m_position.Z);

    private static Vector3 Shared(ulong vertex, Vector4 bbMin, Vector4 bbMax)
    {
        float num1 = (float)(((double)bbMax.X - (double)bbMin.X) / 2097151.0);
        float num2 = (float)(((double)bbMax.Y - (double)bbMin.Y) / 2097151.0);
        float num3 = (float)(((double)bbMax.Z - (double)bbMin.Z) / 4194303.0);
        double x = (double)(vertex & 2097151UL) * (double)num1 + (double)bbMin.X;
        float num4 = (float)(vertex >> 21 & 2097151UL) * num2 + bbMin.Y;
        float num5 = (float)(vertex >> 42 & 4194303UL) * num3 + bbMin.Z;
        return new Vector3((float)x, num4, num5);
    }

    private static Vector3 Packed(uint vertex, Vector3 scale, Vector3 offset)
    {
        double x = (double)(vertex & 2047U) * (double)scale.X + (double)offset.X;
        float num1 = (float)(vertex >> 11 & 2047U) * scale.Y + offset.Y;
        float num2 = (float)(vertex >> 22 & 1023U) * scale.Z + offset.Z;
        return new Vector3((float)x, num1, num2);
    }

    public static (List<Vector3> Mesh, List<Vector3> Graph) Navmesh(HKLib.hk2018.hkRootLevelContainer root)
    {
        var mesh = root.m_namedVariants.Where(v => v.m_className == nameof(HKLib.hk2018.hkaiNavMesh)).Select(v => v.m_variant as HKLib.hk2018.hkaiNavMesh).FirstOrDefault();
        var graph = root.m_namedVariants.Where(v => v.m_className == nameof(HKLib.hk2018.hkaiDirectedGraphExplicitCost)).Select(v => v.m_variant as HKLib.hk2018.hkaiDirectedGraphExplicitCost).FirstOrDefault();
        return (mesh == null ? null : OldNavmesh.Mesh(mesh.m_vertices, mesh.m_faces.Select(f => (f.m_startEdgeIndex, (int)f.m_numEdges)).ToList(), mesh.m_edges.Select(e => e.m_a).ToList()),
                graph == null ? null : OldNavmesh.Graph(graph.m_positions, graph.m_nodes.Select(n => (n.m_startEdgeIndex, n.m_numEdges)).ToList(), graph.m_edges.Select(e => (int)e.m_target).ToList()));
    }
}

public static class OldHkLib2019
{
    /// <summary>HKLib2019_Helper.LoadCollisionMesh.</summary>
    public static List<List<Vector3>> Collision(HKLib.hk2019.hkRootLevelContainer root)
    {
        var result = new List<List<Vector3>>();
        if (root.m_namedVariants.Count == 0) return result;
        var scene = root.m_namedVariants.Select(v => v.m_variant).OfType<HKLib.hk2019.hknpPhysicsSceneData>().FirstOrDefault();
        if (scene == null) return result;
        foreach (var body in scene.m_systemDatas.Where(s => s != null).SelectMany(s => s.m_bodyCinfos))
        {
            try
            {
                var vertices = new List<Vector3>(); var indices = new List<int>();
                Collect(body.m_shape, BodyTransform(body), vertices, indices);
                if (indices.Count > 0) result.Add(indices.Select(i => vertices[i]).ToList());
            }
            catch { }
        }
        return result;
    }

    private static void Collect(HKLib.hk2019.hknpShape shape, Matrix4x4 transform, List<Vector3> vertices, List<int> indices)
    {
        List<Vector3> sv; List<int> si;
        switch (shape)
        {
            case HKLib.hk2019.hknpCompoundShape compound:
                foreach (var inst in compound.m_instances.m_elements.Where(e => e.m_shape != null))
                {
                    var scale = new Vector3(inst.m_scale.X, inst.m_scale.Y, inst.m_scale.Z);
                    if (scale == Vector3.Zero) scale = Vector3.One;
                    Collect(inst.m_shape, Matrix4x4.CreateScale(scale) * inst.m_transform * transform, vertices, indices);
                }
                return;
            case HKLib.hk2019.hknpCompressedMeshShape { m_data: HKLib.hk2019.hknpCompressedMeshShapeData data }:
                (sv, si) = Compressed(data); break;
            case HKLib.hk2019.hknpExternMeshShape { m_geometry: HKLib.hk2019.hknpDefaultExternMeshShapeGeometry { m_geometry: { } g } }:
                sv = g.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
                si = g.m_triangles.SelectMany(t => new[] { t.m_a, t.m_b, t.m_c }).ToList(); break;
            case HKLib.hk2019.hknpConvexShape convex:
                sv = convex.m_hull.m_vertices.Select(v => new Vector3(v.m_x, v.m_y, v.m_z)).ToList();
                si = new List<int>();
                foreach (var face in convex.m_hull.m_faces)
                    for (var i = 1; i < face.m_numIndices - 1; i++)
                    {
                        si.Add(convex.m_hull.m_indices[face.m_firstIndex]);
                        si.Add(convex.m_hull.m_indices[face.m_firstIndex + i]);
                        si.Add(convex.m_hull.m_indices[face.m_firstIndex + i + 1]);
                    }
                break;
            default: return;
        }
        int first = vertices.Count;
        vertices.AddRange(sv.Select(v => Vector3.Transform(v, transform)));
        indices.AddRange(si.Select(i => first + i));
    }

    private static Matrix4x4 BodyTransform(HKLib.hk2019.hknpBodyCinfo body)
    {
        var o = body.m_orientation;
        if (o.LengthSquared() < 1e-6f) o = Quaternion.Identity;
        return Matrix4x4.CreateFromQuaternion(o) * Matrix4x4.CreateTranslation(body.m_position.X, body.m_position.Y, body.m_position.Z);
    }

    private static (List<Vector3>, List<int>) Compressed(HKLib.hk2019.hknpCompressedMeshShapeData data)
    {
        var tree = data.m_meshTree;
        var vertices = new List<Vector3>(); var indices = new List<int>();
        if (tree.m_packedVertices.Count == 0 && tree.m_sharedVertices.Count == 0) return (vertices, indices);
        foreach (var section in tree.m_sections)
        {
            var offset = new Vector3(section.m_codecParms[0], section.m_codecParms[1], section.m_codecParms[2]);
            var scale = new Vector3(section.m_codecParms[3], section.m_codecParms[4], section.m_codecParms[5]);
            bool TryGet(byte index, out Vector3 v)
            {
                v = default;
                if (index < section.m_numPackedVertices)
                {
                    int pi = (int)(index + section.m_firstPackedVertexIndex);
                    if (pi >= tree.m_packedVertices.Count) return false;
                    v = data.DecompressPackedVertex(tree.m_packedVertices[pi], scale, offset); return true;
                }
                int sii = (int)(index + section.m_firstSharedVertexIndex - section.m_numPackedVertices);
                if (sii >= tree.m_sharedVerticesIndex.Count) return false;
                int si = tree.m_sharedVerticesIndex[sii];
                if (si >= tree.m_sharedVertices.Count) return false;
                v = data.DecompressSharedVertex(tree.m_sharedVertices[si], tree.m_domain.m_min, tree.m_domain.m_max); return true;
            }
            for (int i = 0; i < section.m_numPrimitives; i++)
            {
                byte[] p = tree.m_primitives[(int)(i + section.m_firstPrimitiveIndex)].m_indices;
                if (p[0] == 0xDE && p[1] == 0xAD && p[2] == 0xDE && p[3] == 0xAD) continue;
                bool quad = p[2] != p[3];
                if (!TryGet(p[0], out var a) || !TryGet(p[1], out var b) || !TryGet(p[2], out var c)) continue;
                var d = default(Vector3);
                if (quad && !TryGet(p[3], out d)) continue;
                int first = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                indices.AddRange(new[] { first, first + 1, first + 2 });
                if (quad) { vertices.Add(d); indices.AddRange(new[] { first, first + 2, first + 3 }); }
            }
        }
        return (vertices, indices);
    }

    public static (List<Vector3> Mesh, List<Vector3> Graph) Navmesh(HKLib.hk2019.hkRootLevelContainer root)
    {
        var variants = root.m_namedVariants.Select(v => v.m_variant).ToList();
        var mesh = variants.OfType<HKLib.hk2019.hkaiNavMesh>().FirstOrDefault();
        var graph = variants.OfType<HKLib.hk2019.hkaiDirectedGraphExplicitCost>().FirstOrDefault();
        return (mesh == null ? null : OldNavmesh.Mesh(mesh.m_vertices, mesh.m_faces.Select(f => (f.m_startEdgeIndex, (int)f.m_numEdges)).ToList(), mesh.m_edges.Select(e => e.m_a).ToList()),
                graph == null ? null : OldNavmesh.Graph(graph.m_positions, graph.m_nodes.Select(n => (n.m_startEdgeIndex, n.m_numEdges)).ToList(), graph.m_edges.Select(e => (int)e.m_target).ToList()));
    }
}

/// <summary>HavokNavmeshResource's mesh fan and graph lines, shared by its HKX2 and HKLib paths.</summary>
public static class OldNavmesh
{
    public static List<Vector3> Mesh(List<Vector4> verts, List<(int Start, int Count)> faces, List<int> edgeVertices)
    {
        var tris = new List<Vector3>();
        foreach (var (sedge, ecount) in faces)
            for (var t = 0; t < ecount - 2; t++)
            {
                var end = t + 2 >= ecount ? sedge : sedge + t + 2;
                foreach (var e in new[] { sedge, sedge + t + 1, end })
                {
                    Vector4 v = verts[edgeVertices[e]];
                    tris.Add(new Vector3(v.X, v.Y, v.Z));
                }
            }
        return tris;
    }

    public static List<Vector3> Graph(List<Vector4> positions, List<(int Start, int Count)> nodes, List<int> targets)
    {
        var points = new List<Vector3>();
        for (int id = 0; id < nodes.Count; id++)
            for (int e = 0; e < nodes[id].Count; e++)
            {
                Vector4 a = positions[id], b = positions[targets[nodes[id].Start + e]];
                points.Add(new Vector3(a.X, a.Y, a.Z)); points.Add(new Vector3(b.X, b.Y, b.Z));
            }
        return points;
    }
}
