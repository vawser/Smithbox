// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshClearanceCacheManagerRegistrationData : HavokData<hkaiNavMeshClearanceCacheManager.Registration> 
{
    public hkaiNavMeshClearanceCacheManagerRegistrationData(HavokType type, hkaiNavMeshClearanceCacheManager.Registration instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (instance.m_info is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_infoMask":
            case "infoMask":
            {
                if (instance.m_infoMask is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cacheIdentifier":
            case "cacheIdentifier":
            {
                if (instance.m_cacheIdentifier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                if (instance.m_filter is null)
                {
                    return true;
                }
                if (instance.m_filter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is hkaiAstarEdgeFilter castValue)
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
