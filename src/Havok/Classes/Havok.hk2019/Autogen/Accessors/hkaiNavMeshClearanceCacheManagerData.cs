// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshClearanceCacheManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshClearanceCacheManager>
{
    public hkaiNavMeshClearanceCacheManagerData(Havok.hk2019.hkaiNavMeshClearanceCacheManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_layerDatas":
            case "layerDatas":
            {
                value = instance.m_layerDatas;
                return true;
            }
            case "m_defaultOption":
            case "defaultOption":
            {
                value = instance.m_defaultOption;
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
            case "m_layerDatas":
            case "layerDatas":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshClearanceCacheManager.LayerData> castValue) return false;
                instance.m_layerDatas = castValue;
                return true;
            }
            case "m_defaultOption":
            case "defaultOption":
            {
                if (value is Havok.hk2019.hkaiNavMeshClearanceCacheManager.DefaultCachingOption castValue)
                {
                    instance.m_defaultOption = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_defaultOption = (Havok.hk2019.hkaiNavMeshClearanceCacheManager.DefaultCachingOption)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
