// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkFreeListArrayElementHknpMotionPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkFreeListArrayElementHknpMotionProperties>
{
    public hkFreeListArrayElementHknpMotionPropertiesData(Havok.hk2018.hkFreeListArrayElementHknpMotionProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isExclusive":
            case "isExclusive":
            {
                value = instance.m_isExclusive;
                return true;
            }
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
            case "m_timeFactor":
            case "timeFactor":
            {
                value = instance.m_timeFactor;
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
            case "m_solverStabilizationSpeedThreshold":
            case "solverStabilizationSpeedThreshold":
            {
                value = instance.m_solverStabilizationSpeedThreshold;
                return true;
            }
            case "m_solverStabilizationSpeedReduction":
            case "solverStabilizationSpeedReduction":
            {
                value = instance.m_solverStabilizationSpeedReduction;
                return true;
            }
            case "m_deactivationSettings":
            case "deactivationSettings":
            {
                value = instance.m_deactivationSettings;
                return true;
            }
            case "m_fullCastSettings":
            case "fullCastSettings":
            {
                value = instance.m_fullCastSettings;
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
            case "m_isExclusive":
            case "isExclusive":
            {
                if (value is not uint castValue) return false;
                instance.m_isExclusive = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not uint castValue) return false;
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
            case "m_timeFactor":
            case "timeFactor":
            {
                if (value is not float castValue) return false;
                instance.m_timeFactor = castValue;
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
            case "m_solverStabilizationSpeedThreshold":
            case "solverStabilizationSpeedThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_solverStabilizationSpeedThreshold = castValue;
                return true;
            }
            case "m_solverStabilizationSpeedReduction":
            case "solverStabilizationSpeedReduction":
            {
                if (value is not float castValue) return false;
                instance.m_solverStabilizationSpeedReduction = castValue;
                return true;
            }
            case "m_deactivationSettings":
            case "deactivationSettings":
            {
                if (value is not Havok.hk2018.hknpMotionProperties.DeactivationSettings castValue) return false;
                instance.m_deactivationSettings = castValue;
                return true;
            }
            case "m_fullCastSettings":
            case "fullCastSettings":
            {
                if (value is not Havok.hk2018.hknpMotionProperties.FullCastSettings castValue) return false;
                instance.m_fullCastSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
