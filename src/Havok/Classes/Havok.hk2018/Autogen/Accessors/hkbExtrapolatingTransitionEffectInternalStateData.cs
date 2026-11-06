// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbExtrapolatingTransitionEffectInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbExtrapolatingTransitionEffectInternalState>
{
    public hkbExtrapolatingTransitionEffectInternalStateData(Havok.hk2018.hkbExtrapolatingTransitionEffectInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_fromGeneratorSyncInfo":
            case "fromGeneratorSyncInfo":
            {
                value = instance.m_fromGeneratorSyncInfo;
                return true;
            }
            case "m_fromGeneratorPartitionInfo":
            case "fromGeneratorPartitionInfo":
            {
                value = instance.m_fromGeneratorPartitionInfo;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                value = instance.m_worldFromModel;
                return true;
            }
            case "m_motion":
            case "motion":
            {
                value = instance.m_motion;
                return true;
            }
            case "m_pose":
            case "pose":
            {
                value = instance.m_pose;
                return true;
            }
            case "m_additivePose":
            case "additivePose":
            {
                value = instance.m_additivePose;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                value = instance.m_boneWeights;
                return true;
            }
            case "m_toGeneratorDuration":
            case "toGeneratorDuration":
            {
                value = instance.m_toGeneratorDuration;
                return true;
            }
            case "m_isFromGeneratorActive":
            case "isFromGeneratorActive":
            {
                value = instance.m_isFromGeneratorActive;
                return true;
            }
            case "m_gotPose":
            case "gotPose":
            {
                value = instance.m_gotPose;
                return true;
            }
            case "m_gotAdditivePose":
            case "gotAdditivePose":
            {
                value = instance.m_gotAdditivePose;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_fromGeneratorSyncInfo":
            case "fromGeneratorSyncInfo":
            {
                if (value is not Havok.hk2018.hkbGeneratorSyncInfo castValue) return false;
                instance.m_fromGeneratorSyncInfo = castValue;
                return true;
            }
            case "m_fromGeneratorPartitionInfo":
            case "fromGeneratorPartitionInfo":
            {
                if (value is not Havok.hk2018.hkbGeneratorPartitionInfo castValue) return false;
                instance.m_fromGeneratorPartitionInfo = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not Havok.hk2018.hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_motion":
            case "motion":
            {
                if (value is not Havok.hk2018.hkQsTransform castValue) return false;
                instance.m_motion = castValue;
                return true;
            }
            case "m_pose":
            case "pose":
            {
                if (value is not List<Havok.hk2018.hkQsTransform> castValue) return false;
                instance.m_pose = castValue;
                return true;
            }
            case "m_additivePose":
            case "additivePose":
            {
                if (value is not List<Havok.hk2018.hkQsTransform> castValue) return false;
                instance.m_additivePose = castValue;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (value is not List<float> castValue) return false;
                instance.m_boneWeights = castValue;
                return true;
            }
            case "m_toGeneratorDuration":
            case "toGeneratorDuration":
            {
                if (value is not float castValue) return false;
                instance.m_toGeneratorDuration = castValue;
                return true;
            }
            case "m_isFromGeneratorActive":
            case "isFromGeneratorActive":
            {
                if (value is not bool castValue) return false;
                instance.m_isFromGeneratorActive = castValue;
                return true;
            }
            case "m_gotPose":
            case "gotPose":
            {
                if (value is not bool castValue) return false;
                instance.m_gotPose = castValue;
                return true;
            }
            case "m_gotAdditivePose":
            case "gotAdditivePose":
            {
                if (value is not bool castValue) return false;
                instance.m_gotAdditivePose = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
