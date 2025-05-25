// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpLinearSurfaceVelocityData : HavokData<hknpLinearSurfaceVelocity> 
{
    public hknpLinearSurfaceVelocityData(HavokType type, hknpLinearSurfaceVelocity instance) : base(type, instance) {}

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
            case "m_space":
            case "space":
            {
                if (instance.m_space is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_space is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_projectMethod":
            case "projectMethod":
            {
                if (instance.m_projectMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_projectMethod is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxVelocityScale":
            case "maxVelocityScale":
            {
                if (instance.m_maxVelocityScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityMeasurePlane":
            case "velocityMeasurePlane":
            {
                if (instance.m_velocityMeasurePlane is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (instance.m_velocity is not TGet castValue) return false;
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
            case "m_space":
            case "space":
            {
                if (value is hknpSurfaceVelocity.Space castValue)
                {
                    instance.m_space = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_space = (hknpSurfaceVelocity.Space)byteValue;
                    return true;
                }
                return false;
            }
            case "m_projectMethod":
            case "projectMethod":
            {
                if (value is hknpLinearSurfaceVelocity.ProjectMethod castValue)
                {
                    instance.m_projectMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_projectMethod = (hknpLinearSurfaceVelocity.ProjectMethod)byteValue;
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
