// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiWorldCommandsVolumeNavigatorSetLineOfSightTypeData : HavokData<HKLib.hk2019.hkaiWorldCommands.VolumeNavigatorSetLineOfSightType> 
{
    public hkaiWorldCommandsVolumeNavigatorSetLineOfSightTypeData(HavokType type, HKLib.hk2019.hkaiWorldCommands.VolumeNavigatorSetLineOfSightType instance) : base(type, instance) {}

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
            case "m_worldIndex":
            case "worldIndex":
            {
                if (instance.m_worldIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lineOfSightType":
            case "lineOfSightType":
            {
                if (instance.m_lineOfSightType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_lineOfSightType is TGet intValue)
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
                if (value is HKLib.hk2019.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (HKLib.hk2019.hkCommand.PrimaryType)byteValue;
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
            case "m_worldIndex":
            case "worldIndex":
            {
                if (value is not int castValue) return false;
                instance.m_worldIndex = castValue;
                return true;
            }
            case "m_lineOfSightType":
            case "lineOfSightType":
            {
                if (value is HKLib.hk2019.hkaiNavVolumePathSearchParameters.LineOfSightType castValue)
                {
                    instance.m_lineOfSightType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_lineOfSightType = (HKLib.hk2019.hkaiNavVolumePathSearchParameters.LineOfSightType)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
