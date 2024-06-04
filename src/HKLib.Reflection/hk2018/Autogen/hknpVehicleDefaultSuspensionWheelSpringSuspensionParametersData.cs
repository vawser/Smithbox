// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleDefaultSuspensionWheelSpringSuspensionParametersData : HavokData<hknpVehicleDefaultSuspension.WheelSpringSuspensionParameters> 
{
    public hknpVehicleDefaultSuspensionWheelSpringSuspensionParametersData(HavokType type, hknpVehicleDefaultSuspension.WheelSpringSuspensionParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strength":
            case "strength":
            {
                if (instance.m_strength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dampingCompression":
            case "dampingCompression":
            {
                if (instance.m_dampingCompression is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dampingRelaxation":
            case "dampingRelaxation":
            {
                if (instance.m_dampingRelaxation is not TGet castValue) return false;
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
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            case "m_dampingCompression":
            case "dampingCompression":
            {
                if (value is not float castValue) return false;
                instance.m_dampingCompression = castValue;
                return true;
            }
            case "m_dampingRelaxation":
            case "dampingRelaxation":
            {
                if (value is not float castValue) return false;
                instance.m_dampingRelaxation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
