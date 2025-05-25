// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMotionPropertiesDataData : HavokData<HKLib.hk2018.hknpMotionPropertiesData> 
{
    public hknpMotionPropertiesDataData(HavokType type, HKLib.hk2018.hknpMotionPropertiesData instance) : base(type, instance) {}

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
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gravityFactor":
            case "gravityFactor":
            {
                if (instance.m_gravityFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxLinearSpeed":
            case "maxLinearSpeed":
            {
                if (instance.m_maxLinearSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngularSpeed":
            case "maxAngularSpeed":
            {
                if (instance.m_maxAngularSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearDamping":
            case "linearDamping":
            {
                if (instance.m_linearDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularDamping":
            case "angularDamping":
            {
                if (instance.m_angularDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_solverStabilization":
            case "solverStabilization":
            {
                if (instance.m_solverStabilization is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_solverStabilization is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_deactivationReferenceDistance":
            case "deactivationReferenceDistance":
            {
                if (instance.m_deactivationReferenceDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deactivationReferenceRotation":
            case "deactivationReferenceRotation":
            {
                if (instance.m_deactivationReferenceRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deactivationStrategy":
            case "deactivationStrategy":
            {
                if (instance.m_deactivationStrategy is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_deactivationStrategy is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
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
            case "m_flags":
            case "flags":
            {
                if (value is not int castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_gravityFactor":
            case "gravityFactor":
            {
                if (value is not float castValue) return false;
                instance.m_gravityFactor = castValue;
                return true;
            }
            case "m_maxLinearSpeed":
            case "maxLinearSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maxLinearSpeed = castValue;
                return true;
            }
            case "m_maxAngularSpeed":
            case "maxAngularSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngularSpeed = castValue;
                return true;
            }
            case "m_linearDamping":
            case "linearDamping":
            {
                if (value is not float castValue) return false;
                instance.m_linearDamping = castValue;
                return true;
            }
            case "m_angularDamping":
            case "angularDamping":
            {
                if (value is not float castValue) return false;
                instance.m_angularDamping = castValue;
                return true;
            }
            case "m_solverStabilization":
            case "solverStabilization":
            {
                if (value is hknpMotionProperties.SolverStabilizationType castValue)
                {
                    instance.m_solverStabilization = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_solverStabilization = (hknpMotionProperties.SolverStabilizationType)intValue;
                    return true;
                }
                return false;
            }
            case "m_deactivationReferenceDistance":
            case "deactivationReferenceDistance":
            {
                if (value is not float castValue) return false;
                instance.m_deactivationReferenceDistance = castValue;
                return true;
            }
            case "m_deactivationReferenceRotation":
            case "deactivationReferenceRotation":
            {
                if (value is not float castValue) return false;
                instance.m_deactivationReferenceRotation = castValue;
                return true;
            }
            case "m_deactivationStrategy":
            case "deactivationStrategy":
            {
                if (value is hknpMotionProperties.DeactivationSettings.Strategy castValue)
                {
                    instance.m_deactivationStrategy = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_deactivationStrategy = (hknpMotionProperties.DeactivationSettings.Strategy)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
