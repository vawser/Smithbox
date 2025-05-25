// Automatically Generated

namespace HKLib.hk2018;

public class hkaiStreamingCollectionInstanceInfo : IHavokObject
{
    public hkaiNavMeshInstance? m_instance;

    public hkaiNavVolumeInstance? m_volumeInstance;

    public hkaiDirectedGraphInstance? m_clusterGraphInstance;

    public hkaiNavMeshQueryMediator? m_mediator;

    public hkaiNavVolumeMediator? m_volumeMediator;

    public uint m_treeNode;

    public uint m_sectionUid;

    public hkaiReferenceFrame m_sectionReferenceFrame = new();

    public hkAxialTransform m_volumeTransform = new();

    public hkHashMap<int, int> m_streamingSets = new();

    public hkHashMap<int, hkaiStreamingSet?> m_graphStreamingSets = new();

    public hkaiStreamingCollectionInstanceInfo.Flags m_flags;


    [Flags]
    public enum Flags : int
    {
        FLAG_MESH_INSTANCE_IS_NEW = 1,
        FLAG_VOLUME_INSTANCE_IS_NEW = 2,
        FLAG_GRAPH_INSTANCE_IS_NEW = 4
    }

}

