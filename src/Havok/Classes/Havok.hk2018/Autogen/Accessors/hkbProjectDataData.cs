// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbProjectDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbProjectData>
{
    public hkbProjectDataData(Havok.hk2018.hkbProjectData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_worldUpWS":
            case "worldUpWS":
            {
                value = instance.m_worldUpWS;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                value = instance.m_stringData;
                return true;
            }
            case "m_defaultEventMode":
            case "defaultEventMode":
            {
                value = instance.m_defaultEventMode;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_worldUpWS":
            case "worldUpWS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_worldUpWS = castValue;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                if (value is null)
                {
                    instance.m_stringData = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbProjectStringData castValue)
                {
                    instance.m_stringData = castValue;
                    return true;
                }
                return false;
            }
            case "m_defaultEventMode":
            case "defaultEventMode":
            {
                if (value is Havok.hk2018.hkbTransitionEffect.EventMode castValue)
                {
                    instance.m_defaultEventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_defaultEventMode = (Havok.hk2018.hkbTransitionEffect.EventMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
