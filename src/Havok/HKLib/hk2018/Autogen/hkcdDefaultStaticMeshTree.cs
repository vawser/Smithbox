// Automatically Generated

namespace HKLib.hk2018;

public class hkcdDefaultStaticMeshTree : hkcdStaticMeshTree.Base
{
    public List<uint> m_packedVertices = new();

    public List<ulong> m_sharedVertices = new();

    public List<hkcdDefaultStaticMeshTree.PrimitiveDataRun> m_primitiveDataRuns = new();


    public class PrimitiveDataRun : IHavokObject
    {
        public ushort m_value;

        public byte m_index;

        public byte m_count;

    }


}

