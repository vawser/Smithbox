// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshClearanceCacheManager : hkReferencedObject
{
    public List<hkaiNavMeshClearanceCacheManager.LayerData> m_layerDatas = new();

    public hkaiNavMeshClearanceCacheManager.DefaultCachingOption m_defaultOption;


    public enum DefaultCachingOption : int
    {
        DCO_WARN_AND_TREAT_AS_UNFILTERED = 0,
        DCO_TREAT_AS_UNFILTERED = 1,
        DCO_DO_NOT_CACHE = 2,
        DCO_ASSERT = 3
    }

    public enum CachingOption : int
    {
        CACHE = 0,
        USE_UNFILTERED = 1,
        DO_NOT_CACHE = 2,
        NOT_SET = -1
    }

    public class LayerData : IHavokObject
    {
        public List<hkaiNavMeshClearanceCacheManager.Registration> m_registrations = new();

        public List<hkaiNavMeshClearanceCacheManager.CacheInfo> m_cacheInfos = new();

    }


    public class CacheInfo : IHavokObject
    {
        public float m_clearanceCeiling;

    }


    public class Registration : IHavokObject
    {
        public ulong m_id;

        public uint m_info;

        public uint m_infoMask;

        public byte m_cacheIdentifier;

        public hkaiAstarEdgeFilter? m_filter;

    }


}

