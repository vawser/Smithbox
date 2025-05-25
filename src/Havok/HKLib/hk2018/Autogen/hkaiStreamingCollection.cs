// Automatically Generated

namespace HKLib.hk2018;

public class hkaiStreamingCollection : hkReferencedObject
{
    public bool m_isTemporary;

    public hkaiCopyOnWritePtr<hkcdDynamicAabbTree> m_tree = new();

    public List<hkaiStreamingCollectionInstanceInfo> m_instances = new();

    public List<hkaiCopyOnWritePtr<hkaiStreamingSetInstance>> m_streamingSetInstances = new();

    public hkaiNavMeshClearanceCacheManager? m_clearanceCacheManager;

    public readonly float[] m_erosionRadii = new float[32];

    public hkaiDynamicNavMeshQueryMediator? m_dynamicNavMeshMediator;

    public hkaiDynamicNavVolumeMediator? m_dynamicNavVolumeMediator;

}

