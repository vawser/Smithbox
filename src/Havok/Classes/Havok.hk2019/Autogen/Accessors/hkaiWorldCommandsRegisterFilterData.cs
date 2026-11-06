// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiWorldCommandsRegisterFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiWorldCommands.RegisterFilter>
{
    public hkaiWorldCommandsRegisterFilterData(Havok.hk2019.hkaiWorldCommands.RegisterFilter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                value = instance.m_sizePaddedTo16;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                value = instance.m_filterBits;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                value = instance.m_primaryType;
                return true;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                value = instance.m_secondaryType;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                value = instance.m_layerIndex;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                value = instance.m_edgeFilter;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                value = instance.m_filterInfo;
                return true;
            }
            case "m_filterInfoMask":
            case "filterInfoMask":
            {
                value = instance.m_filterInfoMask;
                return true;
            }
            case "m_clearanceCeiling":
            case "clearanceCeiling":
            {
                value = instance.m_clearanceCeiling;
                return true;
            }
            case "m_cachingOption":
            case "cachingOption":
            {
                value = instance.m_cachingOption;
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
                if (value is Havok.hk2019.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (Havok.hk2019.hkCommand.PrimaryType)byteValue;
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
                if (value is Havok.hk2019.hkaiAstarEdgeFilter castValue)
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
                if (value is Havok.hk2019.hkaiNavMeshClearanceCacheManager.CachingOption castValue)
                {
                    instance.m_cachingOption = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_cachingOption = (Havok.hk2019.hkaiNavMeshClearanceCacheManager.CachingOption)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
