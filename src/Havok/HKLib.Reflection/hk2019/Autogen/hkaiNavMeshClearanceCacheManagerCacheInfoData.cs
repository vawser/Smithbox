// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshClearanceCacheManagerCacheInfoData : HavokData<HKLib.hk2019.hkaiNavMeshClearanceCacheManager.CacheInfo> 
{
    public hkaiNavMeshClearanceCacheManagerCacheInfoData(HavokType type, HKLib.hk2019.hkaiNavMeshClearanceCacheManager.CacheInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                if (instance.m_clearanceCeiling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                if (value is not float castValue) return false;
                instance.m_clearanceCeiling = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
