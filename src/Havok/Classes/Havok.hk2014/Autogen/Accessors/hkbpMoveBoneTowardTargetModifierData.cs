// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpMoveBoneTowardTargetModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpMoveBoneTowardTargetModifier>
{
    public hkbpMoveBoneTowardTargetModifierData(Havok.hk2014.hkbpMoveBoneTowardTargetModifier instance) : base(instance) { }

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
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                value = instance.m_targetIn;
                return true;
            }
            case "m_offsetInBoneSpace":
            case "offsetInBoneSpace":
            {
                value = instance.m_offsetInBoneSpace;
                return true;
            }
            case "m_alignAxisBS":
            case "alignAxisBS":
            {
                value = instance.m_alignAxisBS;
                return true;
            }
            case "m_targetAlignAxisTS":
            case "targetAlignAxisTS":
            {
                value = instance.m_targetAlignAxisTS;
                return true;
            }
            case "m_alignWithCharacterForwardBS":
            case "alignWithCharacterForwardBS":
            {
                value = instance.m_alignWithCharacterForwardBS;
                return true;
            }
            case "m_currentBonePositionOut":
            case "currentBonePositionOut":
            {
                value = instance.m_currentBonePositionOut;
                return true;
            }
            case "m_currentBoneRotationOut":
            case "currentBoneRotationOut":
            {
                value = instance.m_currentBoneRotationOut;
                return true;
            }
            case "m_eventToSendWhenTargetReached":
            case "eventToSendWhenTargetReached":
            {
                value = instance.m_eventToSendWhenTargetReached;
                return true;
            }
            case "m_childGenerator":
            case "childGenerator":
            {
                value = instance.m_childGenerator;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_ragdollBoneIndex":
            case "ragdollBoneIndex":
            {
                value = instance.m_ragdollBoneIndex;
                return true;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                value = instance.m_animationBoneIndex;
                return true;
            }
            case "m_targetMode":
            case "targetMode":
            {
                value = instance.m_targetMode;
                return true;
            }
            case "m_alignMode":
            case "alignMode":
            {
                value = instance.m_alignMode;
                return true;
            }
            case "m_useVelocityPrediction":
            case "useVelocityPrediction":
            {
                value = instance.m_useVelocityPrediction;
                return true;
            }
            case "m_affectOrientation":
            case "affectOrientation":
            {
                value = instance.m_affectOrientation;
                return true;
            }
            case "m_currentBoneIsValidOut":
            case "currentBoneIsValidOut":
            {
                value = instance.m_currentBoneIsValidOut;
                return true;
            }
            case "m_finalAnimBonePositionMS":
            case "finalAnimBonePositionMS":
            {
                value = instance.m_finalAnimBonePositionMS;
                return true;
            }
            case "m_initialAnimBonePositionMS":
            case "initialAnimBonePositionMS":
            {
                value = instance.m_initialAnimBonePositionMS;
                return true;
            }
            case "m_finalAnimBoneOrientationMS":
            case "finalAnimBoneOrientationMS":
            {
                value = instance.m_finalAnimBoneOrientationMS;
                return true;
            }
            case "m_animationFromRagdoll":
            case "animationFromRagdoll":
            {
                value = instance.m_animationFromRagdoll;
                return true;
            }
            case "m_totalMotion":
            case "totalMotion":
            {
                value = instance.m_totalMotion;
                return true;
            }
            case "m_accumulatedMotion":
            case "accumulatedMotion":
            {
                value = instance.m_accumulatedMotion;
                return true;
            }
            case "m_useAnimationData":
            case "useAnimationData":
            {
                value = instance.m_useAnimationData;
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                if (value is null)
                {
                    instance.m_targetIn = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbpTarget castValue)
                {
                    instance.m_targetIn = castValue;
                    return true;
                }
                return false;
            }
            case "m_offsetInBoneSpace":
            case "offsetInBoneSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_offsetInBoneSpace = castValue;
                return true;
            }
            case "m_alignAxisBS":
            case "alignAxisBS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_alignAxisBS = castValue;
                return true;
            }
            case "m_targetAlignAxisTS":
            case "targetAlignAxisTS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetAlignAxisTS = castValue;
                return true;
            }
            case "m_alignWithCharacterForwardBS":
            case "alignWithCharacterForwardBS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_alignWithCharacterForwardBS = castValue;
                return true;
            }
            case "m_currentBonePositionOut":
            case "currentBonePositionOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_currentBonePositionOut = castValue;
                return true;
            }
            case "m_currentBoneRotationOut":
            case "currentBoneRotationOut":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_currentBoneRotationOut = castValue;
                return true;
            }
            case "m_eventToSendWhenTargetReached":
            case "eventToSendWhenTargetReached":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_eventToSendWhenTargetReached = castValue;
                return true;
            }
            case "m_childGenerator":
            case "childGenerator":
            {
                if (value is null)
                {
                    instance.m_childGenerator = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbGenerator castValue)
                {
                    instance.m_childGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_ragdollBoneIndex":
            case "ragdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollBoneIndex = castValue;
                return true;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_animationBoneIndex = castValue;
                return true;
            }
            case "m_targetMode":
            case "targetMode":
            {
                if (value is Havok.hk2014.hkbpMoveBoneTowardTargetModifier.TargetModeMBTT castValue)
                {
                    instance.m_targetMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_targetMode = (Havok.hk2014.hkbpMoveBoneTowardTargetModifier.TargetModeMBTT)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignMode":
            case "alignMode":
            {
                if (value is not sbyte castValue) return false;
                instance.m_alignMode = castValue;
                return true;
            }
            case "m_useVelocityPrediction":
            case "useVelocityPrediction":
            {
                if (value is not bool castValue) return false;
                instance.m_useVelocityPrediction = castValue;
                return true;
            }
            case "m_affectOrientation":
            case "affectOrientation":
            {
                if (value is not bool castValue) return false;
                instance.m_affectOrientation = castValue;
                return true;
            }
            case "m_currentBoneIsValidOut":
            case "currentBoneIsValidOut":
            {
                if (value is not bool castValue) return false;
                instance.m_currentBoneIsValidOut = castValue;
                return true;
            }
            case "m_finalAnimBonePositionMS":
            case "finalAnimBonePositionMS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_finalAnimBonePositionMS = castValue;
                return true;
            }
            case "m_initialAnimBonePositionMS":
            case "initialAnimBonePositionMS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_initialAnimBonePositionMS = castValue;
                return true;
            }
            case "m_finalAnimBoneOrientationMS":
            case "finalAnimBoneOrientationMS":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_finalAnimBoneOrientationMS = castValue;
                return true;
            }
            case "m_animationFromRagdoll":
            case "animationFromRagdoll":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_animationFromRagdoll = castValue;
                return true;
            }
            case "m_totalMotion":
            case "totalMotion":
            {
                if (value is not Havok.hk2014.hkQsTransform castValue) return false;
                instance.m_totalMotion = castValue;
                return true;
            }
            case "m_accumulatedMotion":
            case "accumulatedMotion":
            {
                if (value is not Havok.hk2014.hkQsTransform castValue) return false;
                instance.m_accumulatedMotion = castValue;
                return true;
            }
            case "m_useAnimationData":
            case "useAnimationData":
            {
                if (value is not bool castValue) return false;
                instance.m_useAnimationData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
