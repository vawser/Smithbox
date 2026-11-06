// Automatically Generated

namespace Havok.hk2015;

public partial class hkpStorageExtendedMeshShape : hkpExtendedMeshShape
{
    public List<hkpStorageExtendedMeshShape.MeshSubpartStorage?> m_meshstorage = new();

    public List<hkpStorageExtendedMeshShape.ShapeSubpartStorage?> m_shapestorage = new();


    public class Material : hkpMeshMaterial
    {
        public float m_restitution;

        public float m_friction;

        public ulong m_userData;

    }

    public class MeshSubpartStorage : hkReferencedObject
    {
        public List<Vector4> m_vertices = new();

        public List<byte> m_indices8 = new();

        public List<ushort> m_indices16 = new();

        public List<uint> m_indices32 = new();

        public List<byte> m_materialIndices = new();

        public List<hkpStorageExtendedMeshShape.Material> m_materials = new();

        public List<hkpNamedMeshMaterial> m_namedMaterials = new();

        public List<ushort> m_materialIndices16 = new();

    }

    public class ShapeSubpartStorage : hkReferencedObject
    {
        public List<byte> m_materialIndices = new();

        public List<hkpStorageExtendedMeshShape.Material> m_materials = new();

        public List<ushort> m_materialIndices16 = new();

    }

}
