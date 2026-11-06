using System.Numerics;
using Havok.Shared;

namespace Havok.hk55;

// The cross-version collision and navigation views (Havok.Shared/Physics.cs); CollisionGeometry turns them into
// triangles for every SDK alike. Hand-written: the classes are generated as partial for them.

public partial class hkpStorageExtendedMeshShape : IHavokMeshStorageShape
{
    IEnumerable<IHavokTriangleMesh> IHavokMeshStorageShape.MeshStorages =>
        m_meshstorage.Where(s => s != null).Select(s => (IHavokTriangleMesh)new MeshStorageView(s!));

    // a triangle is four indices (the fourth is padding), from the narrowest index list that has any
    private sealed class MeshStorageView(hkpStorageExtendedMeshShapeMeshSubpartStorage storage) : IHavokTriangleMesh
    {
        public int VertexCount => storage.m_vertices.Count;

        public Vector3 GetVertex(int index) => new(storage.m_vertices[index].X, storage.m_vertices[index].Y, storage.m_vertices[index].Z);

        public int TriangleCount => storage.m_indices16.Count > 0 ? storage.m_indices16.Count / 4 : storage.m_indices32.Count / 4;

        public (int A, int B, int C) GetTriangle(int i) => storage.m_indices16.Count > 0
            ? (storage.m_indices16[4 * i], storage.m_indices16[4 * i + 1], storage.m_indices16[4 * i + 2])
            : ((int)storage.m_indices32[4 * i], (int)storage.m_indices32[4 * i + 1], (int)storage.m_indices32[4 * i + 2]);
    }
}
