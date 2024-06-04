// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiWorldCommands;

namespace HKLib.Reflection.hk2018;

internal class hkaiWorldCommandsRegisterFilterData : HavokData<RegisterFilter> 
{
    public hkaiWorldCommandsRegisterFilterData(HavokType type, RegisterFilter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                if (instance.m_sizePaddedTo16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                if (instance.m_filterBits is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                if (instance.m_primaryType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_primaryType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                if (instance.m_secondaryType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (instance.m_layerIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (instance.m_edgeFilter is null)
                {
                    return true;
                }
                if (instance.m_edgeFilter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (instance.m_filterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterInfoMask":
            case "filterInfoMask":
            {
                if (instance.m_filterInfoMask is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                if (instance.m_clearanceCeiling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cachingOption":
            case "cachingOption":
            {
                if (instance.m_cachingOption is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_cachingOption is TGet intValue)
                {
                    value = intValue;
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
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                if (value is not ushort castValue) return false;
                instance.m_sizePaddedTo16 = castValue;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                if (value is not byte castValue) return false;
                instance.m_filterBits = castValue;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                if (value is hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (hkCommand.PrimaryType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                if (value is not ushort castValue) return false;
                instance.m_secondaryType = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (value is null)
                {
                    instance.m_edgeFilter = default;
                    return true;
                }
                if (value is hkaiAstarEdgeFilter castValue)
                {
                    instance.m_edgeFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            case "m_filterInfoMask":
            case "filterInfoMask":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfoMask = castValue;
                return true;
            }
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                if (value is not float castValue) return false;
                instance.m_clearanceCeiling = castValue;
                return true;
            }
            case "m_cachingOption":
            case "cachingOption":
            {
                if (value is hkaiNavMeshClearanceCacheManager.CachingOption castValue)
                {
                    instance.m_cachingOption = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_cachingOption = (hkaiNavMeshClearanceCacheManager.CachingOption)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
