// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleDefaultAerodynamicsData : HavokData<HKLib.hk2019.hknpVehicleDefaultAerodynamics> 
{
    public hknpVehicleDefaultAerodynamicsData(HavokType type, HKLib.hk2019.hknpVehicleDefaultAerodynamics instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_airDensity":
            case "airDensity":
            {
                if (instance.m_airDensity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frontalArea":
            case "frontalArea":
            {
                if (instance.m_frontalArea is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dragCoefficient":
            case "dragCoefficient":
            {
                if (instance.m_dragCoefficient is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_liftCoefficient":
            case "liftCoefficient":
            {
                if (instance.m_liftCoefficient is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extraGravityws":
            case "extraGravityws":
            {
                if (instance.m_extraGravityws is not TGet castValue) return false;
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
            case "m_airDensity":
            case "airDensity":
            {
                if (value is not float castValue) return false;
                instance.m_airDensity = castValue;
                return true;
            }
            case "m_frontalArea":
            case "frontalArea":
            {
                if (value is not float castValue) return false;
                instance.m_frontalArea = castValue;
                return true;
            }
            case "m_dragCoefficient":
            case "dragCoefficient":
            {
                if (value is not float castValue) return false;
                instance.m_dragCoefficient = castValue;
                return true;
            }
            case "m_liftCoefficient":
            case "liftCoefficient":
            {
                if (value is not float castValue) return false;
                instance.m_liftCoefficient = castValue;
                return true;
            }
            case "m_extraGravityws":
            case "extraGravityws":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_extraGravityws = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
