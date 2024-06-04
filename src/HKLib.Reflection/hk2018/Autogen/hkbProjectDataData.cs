// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbProjectDataData : HavokData<hkbProjectData> 
{
    public hkbProjectDataData(HavokType type, hkbProjectData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldUpWS":
            case "worldUpWS":
            {
                if (instance.m_worldUpWS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                if (instance.m_stringData is null)
                {
                    return true;
                }
                if (instance.m_stringData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_defaultEventMode":
            case "defaultEventMode":
            {
                if (instance.m_defaultEventMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_defaultEventMode is TGet sbyteValue)
                {
                    value = sbyteValue;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkbProjectStringData castValue)
                {
                    instance.m_stringData = castValue;
                    return true;
                }
                return false;
            }
            case "m_defaultEventMode":
            case "defaultEventMode":
            {
                if (value is hkbTransitionEffect.EventMode castValue)
                {
                    instance.m_defaultEventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_defaultEventMode = (hkbTransitionEffect.EventMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
