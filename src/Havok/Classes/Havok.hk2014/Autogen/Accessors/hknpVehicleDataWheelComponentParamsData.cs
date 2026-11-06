// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpVehicleDataWheelComponentParamsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpVehicleData.WheelComponentParams>
{
    public hknpVehicleDataWheelComponentParamsData(Havok.hk2014.hknpVehicleData.WheelComponentParams instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_width":
            case "width":
            {
                value = instance.m_width;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                value = instance.m_friction;
                return true;
            }
            case "m_viscosityFriction":
            case "viscosityFriction":
            {
                value = instance.m_viscosityFriction;
                return true;
            }
            case "m_maxFriction":
            case "maxFriction":
            {
                value = instance.m_maxFriction;
                return true;
            }
            case "m_slipAngle":
            case "slipAngle":
            {
                value = instance.m_slipAngle;
                return true;
            }
            case "m_forceFeedbackMultiplier":
            case "forceFeedbackMultiplier":
            {
                value = instance.m_forceFeedbackMultiplier;
                return true;
            }
            case "m_maxContactBodyAcceleration":
            case "maxContactBodyAcceleration":
            {
                value = instance.m_maxContactBodyAcceleration;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                value = instance.m_axle;
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
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_width":
            case "width":
            {
                if (value is not float castValue) return false;
                instance.m_width = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not float castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_viscosityFriction":
            case "viscosityFriction":
            {
                if (value is not float castValue) return false;
                instance.m_viscosityFriction = castValue;
                return true;
            }
            case "m_maxFriction":
            case "maxFriction":
            {
                if (value is not float castValue) return false;
                instance.m_maxFriction = castValue;
                return true;
            }
            case "m_slipAngle":
            case "slipAngle":
            {
                if (value is not float castValue) return false;
                instance.m_slipAngle = castValue;
                return true;
            }
            case "m_forceFeedbackMultiplier":
            case "forceFeedbackMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_forceFeedbackMultiplier = castValue;
                return true;
            }
            case "m_maxContactBodyAcceleration":
            case "maxContactBodyAcceleration":
            {
                if (value is not float castValue) return false;
                instance.m_maxContactBodyAcceleration = castValue;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                if (value is not sbyte castValue) return false;
                instance.m_axle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
