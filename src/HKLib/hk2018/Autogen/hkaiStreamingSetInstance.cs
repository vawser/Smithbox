// Automatically Generated

namespace HKLib.hk2018;

public class hkaiStreamingSetInstance : hkReferencedObject
{
    public readonly int[] m_sectionIndices = new int[2];

    public hkaiStreamingSet? m_streamingSet;

    public List<hkaiStreamingSetInstance.DynUserEdgeConnection> m_dynUserEdgeConnections = new();


    public class DynUserEdgeConnection : IHavokObject
    {
        public readonly int[] m_faceIndices = new int[2];

        public readonly ushort[] m_edgeOffsets = new ushort[2];

    }


}

