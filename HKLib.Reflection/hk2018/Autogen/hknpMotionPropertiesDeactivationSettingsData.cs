// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMotionPropertiesDeactivationSettingsData : HavokData<hknpMotionProperties.DeactivationSettings> 
{
    public hknpMotionPropertiesDeactivationSettingsData(HavokType type, hknpMotionProperties.DeactivationSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxDistSqrd":
            case "maxDistSqrd":
            {
                if (instance.m_maxDistSqrd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRotSqrd":
            case "maxRotSqrd":
            {
                if (instance.m_maxRotSqrd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invBlockSize":
            case "invBlockSize":
            {
                if (instance.m_invBlockSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathingUpperThreshold":
            case "pathingUpperThreshold":
            {
                if (instance.m_pathingUpperThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathingLowerThreshold":
            case "pathingLowerThreshold":
            {
                if (instance.m_pathingLowerThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numDeactivationFrequencyPasses":
            case "numDeactivationFrequencyPasses":
            {
                if (instance.m_numDeactivationFrequencyPasses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deactivationVelocityScaleSquare":
            case "deactivationVelocityScaleSquare":
            {
                if (instance.m_deactivationVelocityScaleSquare is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minimumPathingVelocityScaleSquare":
            case "minimumPathingVelocityScaleSquare":
            {
                if (instance.m_minimumPathingVelocityScaleSquare is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spikingVelocityScaleThresholdSquared":
            case "spikingVelocityScaleThresholdSquared":
            {
                if (instance.m_spikingVelocityScaleThresholdSquared is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minimumSpikingVelocityScaleSquared":
            case "minimumSpikingVelocityScaleSquared":
            {
                if (instance.m_minimumSpikingVelocityScaleSquared is not TGet castValue) return false;
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
