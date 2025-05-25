// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshClearanceCacheSeeding;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshClearanceCacheSeedingCacheDataData : HavokData<CacheData> 
{
    public hkaiNavMeshClearanceCacheSeedingCacheDataData(HavokType type, CacheData instance) : base(type, instance) {}

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
            case "m_initialCache":
            case "initialCache":
            {
                if (instance.m_initialCache is null)
                {
                    return true;
                }
                if (instance.m_initialCache is TGet castValue)
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
            case "m_initialCache":
            case "initialCache":
            {
                if (value is null)
                {
                    instance.m_initialCache = default;
                    return true;
                }
                if (value is hkaiNavMeshClearanceCache castValue)
                {
                    instance.m_initialCache = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
