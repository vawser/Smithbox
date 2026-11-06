// Automatically Generated

namespace Havok.hk2015;

public class hkpStorageMeshShape : hkpMeshShape
{
    public List<hkpStorageMeshShape.SubpartStorage?> m_storage = new();


    public class SubpartStorage : hkReferencedObject
    {
        public List<float> m_vertices = new();

        public List<ushort> m_indices16 = new();

        public List<uint> m_indices32 = new();

        public List<byte> m_materialIndices = new();

        public List<uint> m_materials = new();

        public List<ushort> m_materialIndices16 = new();

    }

}
