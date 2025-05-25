// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkFreeListArrayElementHknpMotionPropertiesData : HavokData<hkFreeListArrayElementHknpMotionProperties> 
{
    public hkFreeListArrayElementHknpMotionPropertiesData(HavokType type, hkFreeListArrayElementHknpMotionProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isExclusive":
            case "isExclusive":
            {
                if (instance.m_isExclusive is not TGet castValue) return false;
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
            case "m_timeFactor":
            case "timeFactor":
            {
                if (instance.m_timeFactor is not TGet castValue) return false;
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
            case "m_solverStabilizationSpeedThreshold":
            case "solverStabilizationSpeedThreshold":
            {
                if (instance.m_solverStabilizationSpeedThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_solverStabilizationSpeedReduction":
            case "solverStabilizationSpeedReduction":
            {
                if (instance.m_solverStabilizationSpeedReduction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deactivationSettings":
            case "deactivationSettings":
            {
                if (instance.m_deactivationSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fullCastSettings":
            case "fullCastSettings":
            {
                if (instance.m_fullCastSettings is not TGet castValue) return false;
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
                if (value is not hknpMotionProperties.DeactivationSettings castValue) return false;
                instance.m_deactivationSettings = castValue;
                return true;
            }
            case "m_fullCastSettings":
            case "fullCastSettings":
            {
                if (value is not hknpMotionProperties.FullCastSettings castValue) return false;
                instance.m_fullCastSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
