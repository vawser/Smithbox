// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiWorldCommandsLoadNavVolumeData : HavokData<HKLib.hk2019.hkaiWorldCommands.LoadNavVolume> 
{
    public hkaiWorldCommandsLoadNavVolumeData(HavokType type, HKLib.hk2019.hkaiWorldCommands.LoadNavVolume instance) : base(type, instance) {}

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
            case "m_navVolumeInstance":
            case "navVolumeInstance":
            {
                if (instance.m_navVolumeInstance is null)
                {
                    return true;
                }
                if (instance.m_navVolumeInstance is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (instance.m_streamingSets is null)
                {
                    return true;
                }
                if (instance.m_streamingSets is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (instance.m_sectionIndex is not TGet castValue) return false;
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
            case "m_navVolumeInstance":
            case "navVolumeInstance":
            {
                if (value is null)
                {
                    instance.m_navVolumeInstance = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavVolumeInstance castValue)
                {
                    instance.m_navVolumeInstance = castValue;
                    return true;
                }
                return false;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (value is null)
                {
                    instance.m_streamingSets = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiReferencedArray<HKLib.hk2019.hkaiStreamingSet?> castValue)
                {
                    instance.m_streamingSets = castValue;
                    return true;
                }
                return false;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_sectionIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
