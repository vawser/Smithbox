// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbPoseMatchingGeneratorData : HavokData<hkbPoseMatchingGenerator> 
{
    public hkbPoseMatchingGeneratorData(HavokType type, hkbPoseMatchingGenerator instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_referencePoseWeightThreshold":
            case "referencePoseWeightThreshold":
            {
                if (instance.m_referencePoseWeightThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendParameter":
            case "blendParameter":
            {
                if (instance.m_blendParameter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minCyclicBlendParameter":
            case "minCyclicBlendParameter":
            {
                if (instance.m_minCyclicBlendParameter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCyclicBlendParameter":
            case "maxCyclicBlendParameter":
            {
                if (instance.m_maxCyclicBlendParameter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexOfSyncMasterChild":
            case "indexOfSyncMasterChild":
            {
                if (instance.m_indexOfSyncMasterChild is not TGet castValue) return false;
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
            case "m_subtractLastChild":
            case "subtractLastChild":
            {
                if (instance.m_subtractLastChild is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (instance.m_children is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldFromModelRotation":
            case "worldFromModelRotation":
            {
                if (instance.m_worldFromModelRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendSpeed":
            case "blendSpeed":
            {
                if (instance.m_blendSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minSpeedToSwitch":
            case "minSpeedToSwitch":
            {
                if (instance.m_minSpeedToSwitch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minSwitchTimeNoError":
            case "minSwitchTimeNoError":
            {
                if (instance.m_minSwitchTimeNoError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minSwitchTimeFullError":
            case "minSwitchTimeFullError":
            {
                if (instance.m_minSwitchTimeFullError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startPlayingEventId":
            case "startPlayingEventId":
            {
                if (instance.m_startPlayingEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startMatchingEventId":
            case "startMatchingEventId":
            {
                if (instance.m_startMatchingEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rootBoneIndex":
            case "rootBoneIndex":
            {
                if (instance.m_rootBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_otherBoneIndex":
            case "otherBoneIndex":
            {
                if (instance.m_otherBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_anotherBoneIndex":
            case "anotherBoneIndex":
            {
                if (instance.m_anotherBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pelvisIndex":
            case "pelvisIndex":
            {
                if (instance.m_pelvisIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (instance.m_mode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_mode is TGet sbyteValue)
                {
                    value = sbyteValue;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_referencePoseWeightThreshold":
            case "referencePoseWeightThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_referencePoseWeightThreshold = castValue;
                return true;
            }
            case "m_blendParameter":
            case "blendParameter":
            {
                if (value is not float castValue) return false;
                instance.m_blendParameter = castValue;
                return true;
            }
            case "m_minCyclicBlendParameter":
            case "minCyclicBlendParameter":
            {
                if (value is not float castValue) return false;
                instance.m_minCyclicBlendParameter = castValue;
                return true;
            }
            case "m_maxCyclicBlendParameter":
            case "maxCyclicBlendParameter":
            {
                if (value is not float castValue) return false;
                instance.m_maxCyclicBlendParameter = castValue;
                return true;
            }
            case "m_indexOfSyncMasterChild":
            case "indexOfSyncMasterChild":
            {
                if (value is not short castValue) return false;
                instance.m_indexOfSyncMasterChild = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not short castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_subtractLastChild":
            case "subtractLastChild":
            {
                if (value is not bool castValue) return false;
                instance.m_subtractLastChild = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not List<hkbBlenderGeneratorChild?> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            case "m_worldFromModelRotation":
            case "worldFromModelRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_worldFromModelRotation = castValue;
                return true;
            }
            case "m_blendSpeed":
            case "blendSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_blendSpeed = castValue;
                return true;
            }
            case "m_minSpeedToSwitch":
            case "minSpeedToSwitch":
            {
                if (value is not float castValue) return false;
                instance.m_minSpeedToSwitch = castValue;
                return true;
            }
            case "m_minSwitchTimeNoError":
            case "minSwitchTimeNoError":
            {
                if (value is not float castValue) return false;
                instance.m_minSwitchTimeNoError = castValue;
                return true;
            }
            case "m_minSwitchTimeFullError":
            case "minSwitchTimeFullError":
            {
                if (value is not float castValue) return false;
                instance.m_minSwitchTimeFullError = castValue;
                return true;
            }
            case "m_startPlayingEventId":
            case "startPlayingEventId":
            {
                if (value is not int castValue) return false;
                instance.m_startPlayingEventId = castValue;
                return true;
            }
            case "m_startMatchingEventId":
            case "startMatchingEventId":
            {
                if (value is not int castValue) return false;
                instance.m_startMatchingEventId = castValue;
                return true;
            }
            case "m_rootBoneIndex":
            case "rootBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_rootBoneIndex = castValue;
                return true;
            }
            case "m_otherBoneIndex":
            case "otherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_otherBoneIndex = castValue;
                return true;
            }
            case "m_anotherBoneIndex":
            case "anotherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_anotherBoneIndex = castValue;
                return true;
            }
            case "m_pelvisIndex":
            case "pelvisIndex":
            {
                if (value is not short castValue) return false;
                instance.m_pelvisIndex = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (value is hkbPoseMatchingGenerator.Mode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_mode = (hkbPoseMatchingGenerator.Mode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
