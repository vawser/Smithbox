// Automatically Generated

namespace HKLib.hk2018;

public class fsnpCustomMeshParameter : hkReferencedObject
{
    public List<fsnpCustomMeshParameter.TriangleData> m_triangleDataArray = new();

    public List<fsnpCustomMeshParameter.PrimitiveData> m_primitiveDataArray = new();

    public int m_vertexDataStride;

    public int m_triangleDataStride;

    public uint m_version;


    public class TriangleData : IHavokObject
    {
        public uint m_primitiveDataIndex;

        public uint m_triangleDataIndex;

        public uint m_vertexIndexA;

        public uint m_vertexIndexB;

        public uint m_vertexIndexC;

    }


    public class PrimitiveData : IHavokObject
    {
        public List<byte> m_vertexData = new();

        public List<byte> m_triangleData = new();

        public List<byte> m_primitiveData = new();

        public uint m_materialNameData;

    }


}

