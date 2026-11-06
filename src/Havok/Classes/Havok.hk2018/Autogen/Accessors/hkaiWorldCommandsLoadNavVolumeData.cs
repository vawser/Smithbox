// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiWorldCommandsLoadNavVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiWorldCommands.LoadNavVolume>
{
    public hkaiWorldCommandsLoadNavVolumeData(Havok.hk2018.hkaiWorldCommands.LoadNavVolume instance) : base(instance) { }

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
            case "m_navVolumeInstance":
            case "navVolumeInstance":
            {
                value = instance.m_navVolumeInstance;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                value = instance.m_mediator;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                value = instance.m_streamingSets;
                return true;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                value = instance.m_sectionIndex;
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
                if (value is Havok.hk2018.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (Havok.hk2018.hkCommand.PrimaryType)byteValue;
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
                if (value is Havok.hk2018.hkaiNavVolumeInstance castValue)
                {
                    instance.m_navVolumeInstance = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (value is null)
                {
                    instance.m_mediator = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavVolumeMediator castValue)
                {
                    instance.m_mediator = castValue;
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
                if (value is Havok.hk2018.hkaiReferencedArray<Havok.hk2018.hkaiStreamingSet?> castValue)
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
