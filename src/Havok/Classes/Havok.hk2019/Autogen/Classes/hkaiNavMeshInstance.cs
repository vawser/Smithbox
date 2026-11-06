// Automatically Generated

namespace Havok.hk2019;

public class hkaiNavMeshInstance : hkReferencedObject
{
    public ulong m_userData;

    public hkaiCopyOnWritePtr<hkaiNavMeshInstanceData> m_instanceData = new();

    public List<hkaiCopyOnWritePtr<hkaiNavMeshClearanceCache>> m_clearanceCaches = new();

    public hkaiReferenceFrame m_referenceFrame = new();

    public hkaiNavMeshInstanceQueryMediator? m_mediator;

    public bool m_used;

}
