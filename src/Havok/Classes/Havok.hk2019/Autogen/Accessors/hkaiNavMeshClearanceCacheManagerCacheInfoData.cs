// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshClearanceCacheManagerCacheInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshClearanceCacheManager.CacheInfo>
{
    public hkaiNavMeshClearanceCacheManagerCacheInfoData(Havok.hk2019.hkaiNavMeshClearanceCacheManager.CacheInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                value = instance.m_clearanceCeiling;
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
