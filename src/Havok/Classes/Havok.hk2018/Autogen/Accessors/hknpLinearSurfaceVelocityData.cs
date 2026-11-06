// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpLinearSurfaceVelocityData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpLinearSurfaceVelocity>
{
    public hknpLinearSurfaceVelocityData(Havok.hk2018.hknpLinearSurfaceVelocity instance) : base(instance) { }

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
            case "m_space":
            case "space":
            {
                value = instance.m_space;
                return true;
            }
            case "m_projectMethod":
            case "projectMethod":
            {
                value = instance.m_projectMethod;
                return true;
            }
            case "m_maxVelocityScale":
            case "maxVelocityScale":
            {
                value = instance.m_maxVelocityScale;
                return true;
            }
            case "m_velocityMeasurePlane":
            case "velocityMeasurePlane":
            {
                value = instance.m_velocityMeasurePlane;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                value = instance.m_velocity;
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
            case "m_space":
            case "space":
            {
                if (value is Havok.hk2018.hknpSurfaceVelocity.Space castValue)
                {
                    instance.m_space = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_space = (Havok.hk2018.hknpSurfaceVelocity.Space)byteValue;
                    return true;
                }
                return false;
            }
            case "m_projectMethod":
            case "projectMethod":
            {
                if (value is Havok.hk2018.hknpLinearSurfaceVelocity.ProjectMethod castValue)
                {
                    instance.m_projectMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_projectMethod = (Havok.hk2018.hknpLinearSurfaceVelocity.ProjectMethod)byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxVelocityScale":
            case "maxVelocityScale":
            {
                if (value is not float castValue) return false;
                instance.m_maxVelocityScale = castValue;
                return true;
            }
            case "m_velocityMeasurePlane":
            case "velocityMeasurePlane":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_velocityMeasurePlane = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_velocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
