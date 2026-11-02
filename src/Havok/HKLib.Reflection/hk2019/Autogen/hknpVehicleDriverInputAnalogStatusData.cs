// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleDriverInputAnalogStatusData : HavokData<HKLib.hk2019.hknpVehicleDriverInputAnalogStatus> 
{
    public hknpVehicleDriverInputAnalogStatusData(HavokType type, HKLib.hk2019.hknpVehicleDriverInputAnalogStatus instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
