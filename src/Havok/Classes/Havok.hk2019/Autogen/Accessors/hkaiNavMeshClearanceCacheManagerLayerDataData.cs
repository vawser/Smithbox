// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshClearanceCacheManagerLayerDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshClearanceCacheManager.LayerData>
{
    public hkaiNavMeshClearanceCacheManagerLayerDataData(Havok.hk2019.hkaiNavMeshClearanceCacheManager.LayerData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_registrations":
            case "registrations":
            {
                value = instance.m_registrations;
                return true;
            }
            case "m_cacheInfos":
            case "cacheInfos":
            {
                value = instance.m_cacheInfos;
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
            case "m_registrations":
            case "registrations":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshClearanceCacheManager.Registration> castValue) return false;
                instance.m_registrations = castValue;
                return true;
            }
            case "m_cacheInfos":
            case "cacheInfos":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshClearanceCacheManager.CacheInfo> castValue) return false;
                instance.m_cacheInfos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
