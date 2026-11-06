// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshClearanceCacheManagerRegistrationData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshClearanceCacheManager.Registration>
{
    public hkaiNavMeshClearanceCacheManagerRegistrationData(Havok.hk2019.hkaiNavMeshClearanceCacheManager.Registration instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_info":
            case "info":
            {
                value = instance.m_info;
                return true;
            }
            case "m_infoMask":
            case "infoMask":
            {
                value = instance.m_infoMask;
                return true;
            }
            case "m_cacheIdentifier":
            case "cacheIdentifier":
            {
                value = instance.m_cacheIdentifier;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                value = instance.m_filter;
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
            case "m_id":
            case "id":
            {
                if (value is not ulong castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (value is not uint castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            case "m_infoMask":
            case "infoMask":
            {
                if (value is not uint castValue) return false;
                instance.m_infoMask = castValue;
                return true;
            }
            case "m_cacheIdentifier":
            case "cacheIdentifier":
            {
                if (value is not byte castValue) return false;
                instance.m_cacheIdentifier = castValue;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                if (value is null)
                {
                    instance.m_filter = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiAstarEdgeFilter castValue)
                {
                    instance.m_filter = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
