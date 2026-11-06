// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshClearanceCacheSeedingCacheDataSetData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshClearanceCacheSeeding.CacheDataSet>
{
    public hkaiNavMeshClearanceCacheSeedingCacheDataSetData(Havok.hk2019.hkaiNavMeshClearanceCacheSeeding.CacheDataSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cacheDatas":
            case "cacheDatas":
            {
                value = instance.m_cacheDatas;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_cacheDatas":
            case "cacheDatas":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshClearanceCacheSeeding.CacheData> castValue) return false;
                instance.m_cacheDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
