// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbProjectDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbProjectData>
{
    public hkbProjectDataData(Havok.hk2014.hkbProjectData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
                if (value is Havok.hk2014.hkbProjectStringData castValue)
                {
                    instance.m_stringData = castValue;
                    return true;
                }
                return false;
            }
            case "m_defaultEventMode":
            case "defaultEventMode":
            {
                if (value is Havok.hk2014.hkbTransitionEffect.EventMode castValue)
                {
                    instance.m_defaultEventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_defaultEventMode = (Havok.hk2014.hkbTransitionEffect.EventMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
