// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpSetBodyOrientationCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpSetBodyOrientationCommand>
{
    public hknpSetBodyOrientationCommandData(Havok.hk2018.hknpSetBodyOrientationCommand instance) : base(instance) { }

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
            case "m_pivot":
            case "pivot":
            {
                value = instance.m_pivot;
                return true;
            }
            case "m_activationMode":
            case "activationMode":
            {
                value = instance.m_activationMode;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                value = instance.m_orientation;
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
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            case "m_pivot":
            case "pivot":
            {
                if (value is Havok.hk2018.hknpWorld.PivotLocation castValue)
                {
                    instance.m_pivot = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_pivot = (Havok.hk2018.hknpWorld.PivotLocation)byteValue;
                    return true;
                }
                return false;
            }
            case "m_activationMode":
            case "activationMode":
            {
                if (value is Havok.hk2018.hknpWorld.ActivationMode castValue)
                {
                    instance.m_activationMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_activationMode = (Havok.hk2018.hknpWorld.ActivationMode)byteValue;
                    return true;
                }
                return false;
            }
            case "m_orientation":
            case "orientation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_orientation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
