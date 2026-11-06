// Automatically Generated

namespace Havok.hk2016;

public class hkcdStaticMeshTreeBase : hkcdStaticTree.DefaultTree5
{
    public int m_numPrimitiveKeys;

    public int m_bitsPerKey;

    public uint m_maxKeyValue;

    public byte m_primitiveStoresIsFlatConvex;

    public List<hkcdStaticMeshTreeBase.Section> m_sections = new();

    public List<hkcdStaticMeshTreeBase.Primitive> m_primitives = new();

    public List<ushort> m_sharedVerticesIndex = new();


    public class Connectivity : IHavokObject
    {
        public List<hkcdStaticMeshTreeBase.Connectivity.SectionHeader> m_headers = new();

        public List<byte> m_localLinks = new();

        public List<uint> m_globalLinks = new();


        public class SectionHeader : IHavokObject
        {
            public uint m_baseLocal;

            public uint m_baseGlobal;

        }

    }

    public class Primitive : IHavokObject
    {
        public readonly byte[] m_indices = new byte[4];

    }

    public class PrimitiveDataRunBaseHknpCompressedMeshShapeTreeDataRunData : IHavokObject
    {
        public hknpCompressedMeshShapeTreeDataRunData m_value = new();

        public byte m_index;

        public byte m_count;

    }

    public class Section : hkcdStaticTree.DefaultTree4
    {
        public readonly float[] m_codecParms = new float[6];

        public uint m_firstPackedVertex;

        public hkcdStaticMeshTreeBase.Section.SharedVertices m_sharedVertices = new();

        public hkcdStaticMeshTreeBase.Section.Primitives m_primitives = new();

        public hkcdStaticMeshTreeBase.Section.DataRuns m_dataRuns = new();

        public byte m_numPackedVertices;

        public byte m_numSharedIndices;

        public ushort m_leafIndex;

        public byte m_page;

        public byte m_flags;

        public byte m_layerData;

        public byte m_unusedData;


        public class DataRuns : IHavokObject
        {
            public uint m_data;

        }

        public class Primitives : IHavokObject
        {
            public uint m_data;

        }

        public class SharedVertices : IHavokObject
        {
            public uint m_data;

        }

    }

}
