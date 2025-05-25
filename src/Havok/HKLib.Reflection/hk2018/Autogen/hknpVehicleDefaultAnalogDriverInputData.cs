// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultAnalogDriverInputData : HavokData<hknpVehicleDefaultAnalogDriverInput> 
{
    public hknpVehicleDefaultAnalogDriverInputData(HavokType type, hknpVehicleDefaultAnalogDriverInput instance) : base(type, instance) {}

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
            case "m_slopeChangePointX":
            case "slopeChangePointX":
            {
                if (instance.m_slopeChangePointX is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialSlope":
            case "initialSlope":
            {
                if (instance.m_initialSlope is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deadZone":
            case "deadZone":
            {
                if (instance.m_deadZone is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_autoReverse":
            case "autoReverse":
            {
                if (instance.m_autoReverse is not TGet castValue) return false;
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
            case "m_slopeChangePointX":
            case "slopeChangePointX":
            {
                if (value is not float castValue) return false;
                instance.m_slopeChangePointX = castValue;
                return true;
            }
            case "m_initialSlope":
            case "initialSlope":
            {
                if (value is not float castValue) return false;
                instance.m_initialSlope = castValue;
                return true;
            }
            case "m_deadZone":
            case "deadZone":
            {
                if (value is not float castValue) return false;
                instance.m_deadZone = castValue;
                return true;
            }
            case "m_autoReverse":
            case "autoReverse":
            {
                if (value is not bool castValue) return false;
                instance.m_autoReverse = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
