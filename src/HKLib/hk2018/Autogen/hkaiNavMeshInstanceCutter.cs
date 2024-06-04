// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshInstanceCutter : hkReferencedObject
{
    public hkaiStreamingCollection? m_streamingCollection;

    public int m_sectionIdx;

    public hkaiNavMeshCutConfiguration m_cutConfiguration = new();

    public List<int> m_addStreamingFaces = new();

    public List<int> m_removeStreamingFaces = new();

    public hkHashSet<int> m_modifiedFaces = new();

    public hkHashSet<int> m_needMatchStreamingFaceIndices = new();

    public List<hkAabb> m_modifiedAabbs = new();

}

