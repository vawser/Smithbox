// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDriverInputAnalogStatusData : HavokData<hknpVehicleDriverInputAnalogStatus> 
{
    public hknpVehicleDriverInputAnalogStatusData(HavokType type, hknpVehicleDriverInputAnalogStatus instance) : base(type, instance) {}

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
            case "m_positionX":
            case "positionX":
            {
                if (instance.m_positionX is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positionY":
            case "positionY":
            {
                if (instance.m_positionY is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handbrakeButtonPressed":
            case "handbrakeButtonPressed":
            {
                if (instance.m_handbrakeButtonPressed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_reverseButtonPressed":
            case "reverseButtonPressed":
            {
                if (instance.m_reverseButtonPressed is not TGet castValue) return false;
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
            case "m_positionX":
            case "positionX":
            {
                if (value is not float castValue) return false;
                instance.m_positionX = castValue;
                return true;
            }
            case "m_positionY":
            case "positionY":
            {
                if (value is not float castValue) return false;
                instance.m_positionY = castValue;
                return true;
            }
            case "m_handbrakeButtonPressed":
            case "handbrakeButtonPressed":
            {
                if (value is not bool castValue) return false;
                instance.m_handbrakeButtonPressed = castValue;
                return true;
            }
            case "m_reverseButtonPressed":
            case "reverseButtonPressed":
            {
                if (value is not bool castValue) return false;
                instance.m_reverseButtonPressed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
