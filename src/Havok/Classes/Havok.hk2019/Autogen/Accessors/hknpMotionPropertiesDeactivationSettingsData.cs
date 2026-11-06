// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMotionPropertiesDeactivationSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMotionProperties.DeactivationSettings>
{
    public hknpMotionPropertiesDeactivationSettingsData(Havok.hk2019.hknpMotionProperties.DeactivationSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
