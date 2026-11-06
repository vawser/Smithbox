// Automatically Generated

namespace Havok.hk2014;

public class hkpSerializedAgentNnEntry : hkReferencedObject
{
    public hkpEntity? m_bodyA;

    public hkpEntity? m_bodyB;

    public ulong m_bodyAId;

    public ulong m_bodyBId;

    public bool m_useEntityIds;

    public hkpSerializedAgentNnEntry.SerializedAgentType m_agentType;

    public hkpSimpleContactConstraintAtom m_atom = new();

    public List<byte> m_propertiesStream = new();

    public List<hkContactPoint> m_contactPoints = new();

    public List<byte> m_cpIdMgr = new();

    public readonly byte[] m_nnEntryData = new byte[192];

    public hkpSerializedTrack1nInfo m_trackInfo = new();

    public readonly byte[] m_endianCheckBuffer = new byte[4];

    public uint m_version;


    public enum SerializedAgentType : int
    {
        INVALID_AGENT_TYPE = 0,
        BOX_BOX_AGENT3 = 1,
        CAPSULE_TRIANGLE_AGENT3 = 2,
        PRED_GSK_AGENT3 = 3,
        PRED_GSK_CYLINDER_AGENT3 = 4,
        CONVEX_LIST_AGENT3 = 5,
        LIST_AGENT3 = 6,
        BV_TREE_AGENT3 = 7,
        COLLECTION_COLLECTION_AGENT3 = 8,
        COLLECTION_AGENT3 = 9
    }

}
