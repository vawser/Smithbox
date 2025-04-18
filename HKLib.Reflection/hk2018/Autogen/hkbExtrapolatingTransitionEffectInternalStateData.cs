// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbExtrapolatingTransitionEffectInternalStateData : HavokData<hkbExtrapolatingTransitionEffectInternalState> 
{
    public hkbExtrapolatingTransitionEffectInternalStateData(HavokType type, hkbExtrapolatingTransitionEffectInternalState instance) : base(type, instance) {}

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
            case "m_fromGeneratorSyncInfo":
            case "fromGeneratorSyncInfo":
            {
                if (instance.m_fromGeneratorSyncInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fromGeneratorPartitionInfo":
            case "fromGeneratorPartitionInfo":
            {
                if (instance.m_fromGeneratorPartitionInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (instance.m_worldFromModel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motion":
            case "motion":
            {
                if (instance.m_motion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pose":
            case "pose":
            {
                if (instance.m_pose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_additivePose":
            case "additivePose":
            {
                if (instance.m_additivePose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (instance.m_boneWeights is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toGeneratorDuration":
            case "toGeneratorDuration":
            {
                if (instance.m_toGeneratorDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isFromGeneratorActive":
            case "isFromGeneratorActive":
            {
                if (instance.m_isFromGeneratorActive is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gotPose":
            case "gotPose":
            {
                if (instance.m_gotPose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gotAdditivePose":
            case "gotAdditivePose":
            {
                if (instance.m_gotAdditivePose is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_fromGeneratorSyncInfo":
            case "fromGeneratorSyncInfo":
            {
                if (value is not hkbGeneratorSyncInfo castValue) return false;
                instance.m_fromGeneratorSyncInfo = castValue;
                return true;
            }
            case "m_fromGeneratorPartitionInfo":
            case "fromGeneratorPartitionInfo":
            {
                if (value is not hkbGeneratorPartitionInfo castValue) return false;
                instance.m_fromGeneratorPartitionInfo = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_motion":
            case "motion":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_motion = castValue;
                return true;
            }
            case "m_pose":
            case "pose":
            {
                if (value is not List<hkQsTransform> castValue) return false;
                instance.m_pose = castValue;
                return true;
            }
            case "m_additivePose":
            case "additivePose":
            {
                if (value is not List<hkQsTransform> castValue) return false;
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
