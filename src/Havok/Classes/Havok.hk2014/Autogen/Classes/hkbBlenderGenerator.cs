// Automatically Generated

namespace Havok.hk2014;

public class hkbBlenderGenerator : hkbGenerator
{
    public float m_referencePoseWeightThreshold;

    public float m_blendParameter;

    public float m_minCyclicBlendParameter;

    public float m_maxCyclicBlendParameter;

    public short m_indexOfSyncMasterChild;

    public short m_flags;

    public bool m_subtractLastChild;

    public List<hkbBlenderGeneratorChild?> m_children = new();


    public class ChildInternalState : IHavokObject
    {
        public bool m_isActive;

        public bool m_syncNextFrame;

    }

}
