// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpContactImpulseEventData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpContactImpulseEvent>
{
    private static readonly System.Reflection.FieldInfo _contactImpulsesInfo = typeof(Havok.hk2019.hknpContactImpulseEvent).GetField("m_contactImpulses")!;
    public hknpContactImpulseEventData(Havok.hk2019.hknpContactImpulseEvent instance) : base(instance) { }

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
            case "m_bodyIds":
            case "bodyIds":
            {
                value = instance.m_bodyIds;
                return true;
            }
            case "m_status":
            case "status":
            {
                value = instance.m_status;
                return true;
            }
            case "m_frictionFactor":
            case "frictionFactor":
            {
                value = instance.m_frictionFactor;
                return true;
            }
            case "m_contactImpulses":
            case "contactImpulses":
            {
                value = instance.m_contactImpulses;
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
            case "m_bodyIds":
            case "bodyIds":
            {
                if (value is not Havok.hk2019.hknpBodyIdPair castValue) return false;
                instance.m_bodyIds = castValue;
                return true;
            }
            case "m_status":
            case "status":
            {
                if (value is Havok.hk2019.hknpContactImpulseEvent.Status castValue)
                {
                    instance.m_status = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_status = (Havok.hk2019.hknpContactImpulseEvent.Status)byteValue;
                    return true;
                }
                return false;
            }
            case "m_frictionFactor":
            case "frictionFactor":
            {
                if (value is not float castValue) return false;
                instance.m_frictionFactor = castValue;
                return true;
            }
            case "m_contactImpulses":
            case "contactImpulses":
            {
                if (value is not float[] castValue || castValue.Length != 4) return false;
                try
                {
                    _contactImpulsesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
