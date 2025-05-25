// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimpleWindActionData : HavokData<hclSimpleWindAction> 
{
    public hclSimpleWindActionData(HavokType type, hclSimpleWindAction instance) : base(type, instance) {}

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
            case "m_windDirection":
            case "windDirection":
            {
                if (instance.m_windDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_windMinSpeed":
            case "windMinSpeed":
            {
                if (instance.m_windMinSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_windMaxSpeed":
            case "windMaxSpeed":
            {
                if (instance.m_windMaxSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_windFrequency":
            case "windFrequency":
            {
                if (instance.m_windFrequency is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumDrag":
            case "maximumDrag":
            {
                if (instance.m_maximumDrag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_airVelocity":
            case "airVelocity":
            {
                if (instance.m_airVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentTime":
            case "currentTime":
            {
                if (instance.m_currentTime is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_windDirection":
            case "windDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_windDirection = castValue;
                return true;
            }
            case "m_windMinSpeed":
            case "windMinSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_windMinSpeed = castValue;
                return true;
            }
            case "m_windMaxSpeed":
            case "windMaxSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_windMaxSpeed = castValue;
                return true;
            }
            case "m_windFrequency":
            case "windFrequency":
            {
                if (value is not float castValue) return false;
                instance.m_windFrequency = castValue;
                return true;
            }
            case "m_maximumDrag":
            case "maximumDrag":
            {
                if (value is not float castValue) return false;
                instance.m_maximumDrag = castValue;
                return true;
            }
            case "m_airVelocity":
            case "airVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_airVelocity = castValue;
                return true;
            }
            case "m_currentTime":
            case "currentTime":
            {
                if (value is not float castValue) return false;
                instance.m_currentTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
