// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeGenerationSettingsMergingSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumeGenerationSettings.MergingSettings>
{
    public hkaiNavVolumeGenerationSettingsMergingSettingsData(Havok.hk2019.hkaiNavVolumeGenerationSettings.MergingSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodeWeight":
            case "nodeWeight":
            {
                value = instance.m_nodeWeight;
                return true;
            }
            case "m_edgeWeight":
            case "edgeWeight":
            {
                value = instance.m_edgeWeight;
                return true;
            }
            case "m_estimateNewEdges":
            case "estimateNewEdges":
            {
                value = instance.m_estimateNewEdges;
                return true;
            }
            case "m_iterationsStabilizationThreshold":
            case "iterationsStabilizationThreshold":
            {
                value = instance.m_iterationsStabilizationThreshold;
                return true;
            }
            case "m_maxMergingIterations":
            case "maxMergingIterations":
            {
                value = instance.m_maxMergingIterations;
                return true;
            }
            case "m_randomSeed":
            case "randomSeed":
            {
                value = instance.m_randomSeed;
                return true;
            }
            case "m_multiplier":
            case "multiplier":
            {
                value = instance.m_multiplier;
                return true;
            }
            case "m_useSimpleFirstMergePass":
            case "useSimpleFirstMergePass":
            {
                value = instance.m_useSimpleFirstMergePass;
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
            case "m_nodeWeight":
            case "nodeWeight":
            {
                if (value is not float castValue) return false;
                instance.m_nodeWeight = castValue;
                return true;
            }
            case "m_edgeWeight":
            case "edgeWeight":
            {
                if (value is not float castValue) return false;
                instance.m_edgeWeight = castValue;
                return true;
            }
            case "m_estimateNewEdges":
            case "estimateNewEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_estimateNewEdges = castValue;
                return true;
            }
            case "m_iterationsStabilizationThreshold":
            case "iterationsStabilizationThreshold":
            {
                if (value is not int castValue) return false;
                instance.m_iterationsStabilizationThreshold = castValue;
                return true;
            }
            case "m_maxMergingIterations":
            case "maxMergingIterations":
            {
                if (value is not int castValue) return false;
                instance.m_maxMergingIterations = castValue;
                return true;
            }
            case "m_randomSeed":
            case "randomSeed":
            {
                if (value is not int castValue) return false;
                instance.m_randomSeed = castValue;
                return true;
            }
            case "m_multiplier":
            case "multiplier":
            {
                if (value is not float castValue) return false;
                instance.m_multiplier = castValue;
                return true;
            }
            case "m_useSimpleFirstMergePass":
            case "useSimpleFirstMergePass":
            {
                if (value is not bool castValue) return false;
                instance.m_useSimpleFirstMergePass = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
