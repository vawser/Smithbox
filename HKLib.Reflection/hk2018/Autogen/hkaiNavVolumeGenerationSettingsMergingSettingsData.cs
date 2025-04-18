// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeGenerationSettingsMergingSettingsData : HavokData<hkaiNavVolumeGenerationSettings.MergingSettings> 
{
    public hkaiNavVolumeGenerationSettingsMergingSettingsData(HavokType type, hkaiNavVolumeGenerationSettings.MergingSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodeWeight":
            case "nodeWeight":
            {
                if (instance.m_nodeWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeWeight":
            case "edgeWeight":
            {
                if (instance.m_edgeWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_estimateNewEdges":
            case "estimateNewEdges":
            {
                if (instance.m_estimateNewEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_iterationsStabilizationThreshold":
            case "iterationsStabilizationThreshold":
            {
                if (instance.m_iterationsStabilizationThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxMergingIterations":
            case "maxMergingIterations":
            {
                if (instance.m_maxMergingIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_randomSeed":
            case "randomSeed":
            {
                if (instance.m_randomSeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_multiplier":
            case "multiplier":
            {
                if (instance.m_multiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useSimpleFirstMergePass":
            case "useSimpleFirstMergePass":
            {
                if (instance.m_useSimpleFirstMergePass is not TGet castValue) return false;
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
