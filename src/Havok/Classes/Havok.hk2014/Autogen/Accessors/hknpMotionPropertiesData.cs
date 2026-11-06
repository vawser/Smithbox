// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpMotionPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpMotionProperties>
{
    public hknpMotionPropertiesData(Havok.hk2014.hknpMotionProperties instance) : base(instance) { }

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
            case "m_maxDistSqrd":
            case "maxDistSqrd":
            {
                value = instance.m_maxDistSqrd;
                return true;
            }
            case "m_maxRotSqrd":
            case "maxRotSqrd":
            {
                value = instance.m_maxRotSqrd;
                return true;
            }
            case "m_invBlockSize":
            case "invBlockSize":
            {
                value = instance.m_invBlockSize;
                return true;
            }
            case "m_pathingUpperThreshold":
            case "pathingUpperThreshold":
            {
                value = instance.m_pathingUpperThreshold;
                return true;
            }
            case "m_pathingLowerThreshold":
            case "pathingLowerThreshold":
            {
                value = instance.m_pathingLowerThreshold;
                return true;
            }
            case "m_numDeactivationFrequencyPasses":
            case "numDeactivationFrequencyPasses":
            {
                value = instance.m_numDeactivationFrequencyPasses;
                return true;
            }
            case "m_deactivationVelocityScaleSquare":
            case "deactivationVelocityScaleSquare":
            {
                value = instance.m_deactivationVelocityScaleSquare;
                return true;
            }
            case "m_minimumPathingVelocityScaleSquare":
            case "minimumPathingVelocityScaleSquare":
            {
                value = instance.m_minimumPathingVelocityScaleSquare;
                return true;
            }
            case "m_spikingVelocityScaleThresholdSquared":
            case "spikingVelocityScaleThresholdSquared":
            {
                value = instance.m_spikingVelocityScaleThresholdSquared;
                return true;
            }
            case "m_minimumSpikingVelocityScaleSquared":
            case "minimumSpikingVelocityScaleSquared":
            {
                value = instance.m_minimumSpikingVelocityScaleSquared;
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
                if (value is Havok.hk2014.hknpMotionProperties.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (Havok.hk2014.hknpMotionProperties.FlagsEnum)uintValue;
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
            case "m_maxDistSqrd":
            case "maxDistSqrd":
            {
                if (value is not float castValue) return false;
                instance.m_maxDistSqrd = castValue;
                return true;
            }
            case "m_maxRotSqrd":
            case "maxRotSqrd":
            {
                if (value is not float castValue) return false;
                instance.m_maxRotSqrd = castValue;
                return true;
            }
            case "m_invBlockSize":
            case "invBlockSize":
            {
                if (value is not float castValue) return false;
                instance.m_invBlockSize = castValue;
                return true;
            }
            case "m_pathingUpperThreshold":
            case "pathingUpperThreshold":
            {
                if (value is not short castValue) return false;
                instance.m_pathingUpperThreshold = castValue;
                return true;
            }
            case "m_pathingLowerThreshold":
            case "pathingLowerThreshold":
            {
                if (value is not short castValue) return false;
                instance.m_pathingLowerThreshold = castValue;
                return true;
            }
            case "m_numDeactivationFrequencyPasses":
            case "numDeactivationFrequencyPasses":
            {
                if (value is not byte castValue) return false;
                instance.m_numDeactivationFrequencyPasses = castValue;
                return true;
            }
            case "m_deactivationVelocityScaleSquare":
            case "deactivationVelocityScaleSquare":
            {
                if (value is not byte castValue) return false;
                instance.m_deactivationVelocityScaleSquare = castValue;
                return true;
            }
            case "m_minimumPathingVelocityScaleSquare":
            case "minimumPathingVelocityScaleSquare":
            {
                if (value is not byte castValue) return false;
                instance.m_minimumPathingVelocityScaleSquare = castValue;
                return true;
            }
            case "m_spikingVelocityScaleThresholdSquared":
            case "spikingVelocityScaleThresholdSquared":
            {
                if (value is not byte castValue) return false;
                instance.m_spikingVelocityScaleThresholdSquared = castValue;
                return true;
            }
            case "m_minimumSpikingVelocityScaleSquared":
            case "minimumSpikingVelocityScaleSquared":
            {
                if (value is not byte castValue) return false;
                instance.m_minimumSpikingVelocityScaleSquared = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
