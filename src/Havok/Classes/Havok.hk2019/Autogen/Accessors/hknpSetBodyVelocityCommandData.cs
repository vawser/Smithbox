// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpSetBodyVelocityCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpSetBodyVelocityCommand>
{
    public hknpSetBodyVelocityCommandData(Havok.hk2019.hknpSetBodyVelocityCommand instance) : base(instance) { }

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
            case "m_linVelocity":
            case "linVelocity":
            {
                value = instance.m_linVelocity;
                return true;
            }
            case "m_angVelocity":
            case "angVelocity":
            {
                value = instance.m_angVelocity;
                return true;
            }
            case "m_activationMode":
            case "activationMode":
            {
                value = instance.m_activationMode;
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
            case "m_linVelocity":
            case "linVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linVelocity = castValue;
                return true;
            }
            case "m_angVelocity":
            case "angVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angVelocity = castValue;
                return true;
            }
            case "m_activationMode":
            case "activationMode":
            {
                if (value is Havok.hk2019.hknpWorldWriter.ActivationMode castValue)
                {
                    instance.m_activationMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_activationMode = (Havok.hk2019.hknpWorldWriter.ActivationMode)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
