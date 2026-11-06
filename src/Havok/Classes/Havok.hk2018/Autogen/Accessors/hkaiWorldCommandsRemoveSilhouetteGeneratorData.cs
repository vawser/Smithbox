// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiWorldCommandsRemoveSilhouetteGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiWorldCommands.RemoveSilhouetteGenerator>
{
    public hkaiWorldCommandsRemoveSilhouetteGeneratorData(Havok.hk2018.hkaiWorldCommands.RemoveSilhouetteGenerator instance) : base(instance) { }

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
            case "m_silhouetteGenerator":
            case "silhouetteGenerator":
            {
                value = instance.m_silhouetteGenerator;
                return true;
            }
            case "m_index":
            case "index":
            {
                value = instance.m_index;
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
            case "m_silhouetteGenerator":
            case "silhouetteGenerator":
            {
                if (value is null)
                {
                    instance.m_silhouetteGenerator = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiSilhouetteGenerator castValue)
                {
                    instance.m_silhouetteGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
