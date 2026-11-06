// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpVehicleDefaultAerodynamicsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpVehicleDefaultAerodynamics>
{
    public hknpVehicleDefaultAerodynamicsData(Havok.hk2018.hknpVehicleDefaultAerodynamics instance) : base(instance) { }

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
            case "m_airDensity":
            case "airDensity":
            {
                value = instance.m_airDensity;
                return true;
            }
            case "m_frontalArea":
            case "frontalArea":
            {
                value = instance.m_frontalArea;
                return true;
            }
            case "m_dragCoefficient":
            case "dragCoefficient":
            {
                value = instance.m_dragCoefficient;
                return true;
            }
            case "m_liftCoefficient":
            case "liftCoefficient":
            {
                value = instance.m_liftCoefficient;
                return true;
            }
            case "m_extraGravityws":
            case "extraGravityws":
            {
                value = instance.m_extraGravityws;
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
