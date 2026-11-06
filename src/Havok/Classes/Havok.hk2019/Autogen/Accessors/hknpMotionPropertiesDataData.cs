// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMotionPropertiesDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMotionPropertiesData>
{
    public hknpMotionPropertiesDataData(Havok.hk2019.hknpMotionPropertiesData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_gravityFactor":
            case "gravityFactor":
            {
                value = instance.m_gravityFactor;
                return true;
            }
            case "m_maxLinearSpeed":
            case "maxLinearSpeed":
            {
                value = instance.m_maxLinearSpeed;
                return true;
            }
            case "m_maxAngularSpeed":
            case "maxAngularSpeed":
            {
                value = instance.m_maxAngularSpeed;
                return true;
            }
            case "m_linearDamping":
            case "linearDamping":
            {
                value = instance.m_linearDamping;
                return true;
            }
            case "m_angularDamping":
            case "angularDamping":
            {
                value = instance.m_angularDamping;
                return true;
            }
            case "m_solverStabilization":
            case "solverStabilization":
            {
                value = instance.m_solverStabilization;
                return true;
            }
            case "m_deactivationReferenceDistance":
            case "deactivationReferenceDistance":
            {
                value = instance.m_deactivationReferenceDistance;
                return true;
            }
            case "m_deactivationReferenceRotation":
            case "deactivationReferenceRotation":
            {
                value = instance.m_deactivationReferenceRotation;
                return true;
            }
            case "m_deactivationStrategy":
            case "deactivationStrategy":
            {
                value = instance.m_deactivationStrategy;
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
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2019.hknpCollisionFlags.Enum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_flags = (Havok.hk2019.hknpCollisionFlags.Enum)intValue;
                    return true;
                }
                return false;
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
                if (value is Havok.hk2019.hknpMotionProperties.SolverStabilizationType castValue)
                {
                    instance.m_solverStabilization = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_solverStabilization = (Havok.hk2019.hknpMotionProperties.SolverStabilizationType)intValue;
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
                if (value is Havok.hk2019.hknpMotionProperties.DeactivationSettings.Strategy castValue)
                {
                    instance.m_deactivationStrategy = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_deactivationStrategy = (Havok.hk2019.hknpMotionProperties.DeactivationSettings.Strategy)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
