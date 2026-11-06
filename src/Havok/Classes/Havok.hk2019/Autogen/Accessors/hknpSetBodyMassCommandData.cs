// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpSetBodyMassCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpSetBodyMassCommand>
{
    public hknpSetBodyMassCommandData(Havok.hk2019.hknpSetBodyMassCommand instance) : base(instance) { }

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
            case "m_bodyId":
            case "bodyId":
            {
                value = instance.m_bodyId;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                value = instance.m_mode;
                return true;
            }
            case "m_massOrDensity":
            case "massOrDensity":
            {
                value = instance.m_massOrDensity;
                return true;
            }
            case "m_updateMotionMode":
            case "updateMotionMode":
            {
                value = instance.m_updateMotionMode;
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
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (value is Havok.hk2019.hknpSetBodyMassCommand.Mode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_mode = (Havok.hk2019.hknpSetBodyMassCommand.Mode)intValue;
                    return true;
                }
                return false;
            }
            case "m_massOrDensity":
            case "massOrDensity":
            {
                if (value is not float castValue) return false;
                instance.m_massOrDensity = castValue;
                return true;
            }
            case "m_updateMotionMode":
            case "updateMotionMode":
            {
                if (value is Havok.hk2019.hknpWorldWriter.UpdateMotionMode castValue)
                {
                    instance.m_updateMotionMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_updateMotionMode = (Havok.hk2019.hknpWorldWriter.UpdateMotionMode)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
