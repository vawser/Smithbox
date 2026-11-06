// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbPoseMatchingGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbPoseMatchingGenerator>
{
    public hkbPoseMatchingGeneratorData(Havok.hk2014.hkbPoseMatchingGenerator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_referencePoseWeightThreshold":
            case "referencePoseWeightThreshold":
            {
                value = instance.m_referencePoseWeightThreshold;
                return true;
            }
            case "m_blendParameter":
            case "blendParameter":
            {
                value = instance.m_blendParameter;
                return true;
            }
            case "m_minCyclicBlendParameter":
            case "minCyclicBlendParameter":
            {
                value = instance.m_minCyclicBlendParameter;
                return true;
            }
            case "m_maxCyclicBlendParameter":
            case "maxCyclicBlendParameter":
            {
                value = instance.m_maxCyclicBlendParameter;
                return true;
            }
            case "m_indexOfSyncMasterChild":
            case "indexOfSyncMasterChild":
            {
                value = instance.m_indexOfSyncMasterChild;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_subtractLastChild":
            case "subtractLastChild":
            {
                value = instance.m_subtractLastChild;
                return true;
            }
            case "m_children":
            case "children":
            {
                value = instance.m_children;
                return true;
            }
            case "m_worldFromModelRotation":
            case "worldFromModelRotation":
            {
                value = instance.m_worldFromModelRotation;
                return true;
            }
            case "m_blendSpeed":
            case "blendSpeed":
            {
                value = instance.m_blendSpeed;
                return true;
            }
            case "m_minSpeedToSwitch":
            case "minSpeedToSwitch":
            {
                value = instance.m_minSpeedToSwitch;
                return true;
            }
            case "m_minSwitchTimeNoError":
            case "minSwitchTimeNoError":
            {
                value = instance.m_minSwitchTimeNoError;
                return true;
            }
            case "m_minSwitchTimeFullError":
            case "minSwitchTimeFullError":
            {
                value = instance.m_minSwitchTimeFullError;
                return true;
            }
            case "m_startPlayingEventId":
            case "startPlayingEventId":
            {
                value = instance.m_startPlayingEventId;
                return true;
            }
            case "m_startMatchingEventId":
            case "startMatchingEventId":
            {
                value = instance.m_startMatchingEventId;
                return true;
            }
            case "m_rootBoneIndex":
            case "rootBoneIndex":
            {
                value = instance.m_rootBoneIndex;
                return true;
            }
            case "m_otherBoneIndex":
            case "otherBoneIndex":
            {
                value = instance.m_otherBoneIndex;
                return true;
            }
            case "m_anotherBoneIndex":
            case "anotherBoneIndex":
            {
                value = instance.m_anotherBoneIndex;
                return true;
            }
            case "m_pelvisIndex":
            case "pelvisIndex":
            {
                value = instance.m_pelvisIndex;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                value = instance.m_mode;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
                if (value is not List<Havok.hk2014.hkbBlenderGeneratorChild?> castValue) return false;
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
                if (value is Havok.hk2014.hkbPoseMatchingGenerator.Mode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_mode = (Havok.hk2014.hkbPoseMatchingGenerator.Mode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
