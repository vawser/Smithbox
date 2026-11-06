// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpCatchFallModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpCatchFallModifier>
{
    private static readonly System.Reflection.FieldInfo _catchFallPosInBSInfo = typeof(Havok.hk2014.hkbpCatchFallModifier).GetField("m_catchFallPosInBS")!;
    private static readonly System.Reflection.FieldInfo _currentReachGainInfo = typeof(Havok.hk2014.hkbpCatchFallModifier).GetField("m_currentReachGain")!;
    private static readonly System.Reflection.FieldInfo _catchFallPosIsValidInfo = typeof(Havok.hk2014.hkbpCatchFallModifier).GetField("m_catchFallPosIsValid")!;
    public hkbpCatchFallModifierData(Havok.hk2014.hkbpCatchFallModifier instance) : base(instance) { }

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
            case "m_directionOfFallForwardLS":
            case "directionOfFallForwardLS":
            {
                value = instance.m_directionOfFallForwardLS;
                return true;
            }
            case "m_directionOfFallRightLS":
            case "directionOfFallRightLS":
            {
                value = instance.m_directionOfFallRightLS;
                return true;
            }
            case "m_directionOfFallUpLS":
            case "directionOfFallUpLS":
            {
                value = instance.m_directionOfFallUpLS;
                return true;
            }
            case "m_spineIndices":
            case "spineIndices":
            {
                value = instance.m_spineIndices;
                return true;
            }
            case "m_leftHand":
            case "leftHand":
            {
                value = instance.m_leftHand;
                return true;
            }
            case "m_rightHand":
            case "rightHand":
            {
                value = instance.m_rightHand;
                return true;
            }
            case "m_catchFallDoneEvent":
            case "catchFallDoneEvent":
            {
                value = instance.m_catchFallDoneEvent;
                return true;
            }
            case "m_spreadHandsMultiplier":
            case "spreadHandsMultiplier":
            {
                value = instance.m_spreadHandsMultiplier;
                return true;
            }
            case "m_radarRange":
            case "radarRange":
            {
                value = instance.m_radarRange;
                return true;
            }
            case "m_previousTargetBlendWeight":
            case "previousTargetBlendWeight":
            {
                value = instance.m_previousTargetBlendWeight;
                return true;
            }
            case "m_handsBendDistance":
            case "handsBendDistance":
            {
                value = instance.m_handsBendDistance;
                return true;
            }
            case "m_maxReachDistanceForward":
            case "maxReachDistanceForward":
            {
                value = instance.m_maxReachDistanceForward;
                return true;
            }
            case "m_maxReachDistanceBackward":
            case "maxReachDistanceBackward":
            {
                value = instance.m_maxReachDistanceBackward;
                return true;
            }
            case "m_fadeInReachGainSpeed":
            case "fadeInReachGainSpeed":
            {
                value = instance.m_fadeInReachGainSpeed;
                return true;
            }
            case "m_fadeOutReachGainSpeed":
            case "fadeOutReachGainSpeed":
            {
                value = instance.m_fadeOutReachGainSpeed;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                value = instance.m_fadeOutDuration;
                return true;
            }
            case "m_fadeInTwistSpeed":
            case "fadeInTwistSpeed":
            {
                value = instance.m_fadeInTwistSpeed;
                return true;
            }
            case "m_fadeOutTwistSpeed":
            case "fadeOutTwistSpeed":
            {
                value = instance.m_fadeOutTwistSpeed;
                return true;
            }
            case "m_raycastLayer":
            case "raycastLayer":
            {
                value = instance.m_raycastLayer;
                return true;
            }
            case "m_velocityRagdollBoneIndex":
            case "velocityRagdollBoneIndex":
            {
                value = instance.m_velocityRagdollBoneIndex;
                return true;
            }
            case "m_directionOfFallRagdollBoneIndex":
            case "directionOfFallRagdollBoneIndex":
            {
                value = instance.m_directionOfFallRagdollBoneIndex;
                return true;
            }
            case "m_orientHands":
            case "orientHands":
            {
                value = instance.m_orientHands;
                return true;
            }
            case "m_catchFallPosInBS":
            case "catchFallPosInBS":
            {
                value = instance.m_catchFallPosInBS;
                return true;
            }
            case "m_currentReachGain":
            case "currentReachGain":
            {
                value = instance.m_currentReachGain;
                return true;
            }
            case "m_timeSinceLastModify":
            case "timeSinceLastModify":
            {
                value = instance.m_timeSinceLastModify;
                return true;
            }
            case "m_currentTwistGain":
            case "currentTwistGain":
            {
                value = instance.m_currentTwistGain;
                return true;
            }
            case "m_currentTwistDirection":
            case "currentTwistDirection":
            {
                value = instance.m_currentTwistDirection;
                return true;
            }
            case "m_catchFallPosIsValid":
            case "catchFallPosIsValid":
            {
                value = instance.m_catchFallPosIsValid;
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
            case "m_directionOfFallForwardLS":
            case "directionOfFallForwardLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_directionOfFallForwardLS = castValue;
                return true;
            }
            case "m_directionOfFallRightLS":
            case "directionOfFallRightLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_directionOfFallRightLS = castValue;
                return true;
            }
            case "m_directionOfFallUpLS":
            case "directionOfFallUpLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_directionOfFallUpLS = castValue;
                return true;
            }
            case "m_spineIndices":
            case "spineIndices":
            {
                if (value is null)
                {
                    instance.m_spineIndices = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbBoneIndexArray castValue)
                {
                    instance.m_spineIndices = castValue;
                    return true;
                }
                return false;
            }
            case "m_leftHand":
            case "leftHand":
            {
                if (value is not Havok.hk2014.hkbpCatchFallModifierHand castValue) return false;
                instance.m_leftHand = castValue;
                return true;
            }
            case "m_rightHand":
            case "rightHand":
            {
                if (value is not Havok.hk2014.hkbpCatchFallModifierHand castValue) return false;
                instance.m_rightHand = castValue;
                return true;
            }
            case "m_catchFallDoneEvent":
            case "catchFallDoneEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_catchFallDoneEvent = castValue;
                return true;
            }
            case "m_spreadHandsMultiplier":
            case "spreadHandsMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_spreadHandsMultiplier = castValue;
                return true;
            }
            case "m_radarRange":
            case "radarRange":
            {
                if (value is not float castValue) return false;
                instance.m_radarRange = castValue;
                return true;
            }
            case "m_previousTargetBlendWeight":
            case "previousTargetBlendWeight":
            {
                if (value is not float castValue) return false;
                instance.m_previousTargetBlendWeight = castValue;
                return true;
            }
            case "m_handsBendDistance":
            case "handsBendDistance":
            {
                if (value is not float castValue) return false;
                instance.m_handsBendDistance = castValue;
                return true;
            }
            case "m_maxReachDistanceForward":
            case "maxReachDistanceForward":
            {
                if (value is not float castValue) return false;
                instance.m_maxReachDistanceForward = castValue;
                return true;
            }
            case "m_maxReachDistanceBackward":
            case "maxReachDistanceBackward":
            {
                if (value is not float castValue) return false;
                instance.m_maxReachDistanceBackward = castValue;
                return true;
            }
            case "m_fadeInReachGainSpeed":
            case "fadeInReachGainSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInReachGainSpeed = castValue;
                return true;
            }
            case "m_fadeOutReachGainSpeed":
            case "fadeOutReachGainSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutReachGainSpeed = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_fadeInTwistSpeed":
            case "fadeInTwistSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInTwistSpeed = castValue;
                return true;
            }
            case "m_fadeOutTwistSpeed":
            case "fadeOutTwistSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutTwistSpeed = castValue;
                return true;
            }
            case "m_raycastLayer":
            case "raycastLayer":
            {
                if (value is not short castValue) return false;
                instance.m_raycastLayer = castValue;
                return true;
            }
            case "m_velocityRagdollBoneIndex":
            case "velocityRagdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_velocityRagdollBoneIndex = castValue;
                return true;
            }
            case "m_directionOfFallRagdollBoneIndex":
            case "directionOfFallRagdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_directionOfFallRagdollBoneIndex = castValue;
                return true;
            }
            case "m_orientHands":
            case "orientHands":
            {
                if (value is not bool castValue) return false;
                instance.m_orientHands = castValue;
                return true;
            }
            case "m_catchFallPosInBS":
            case "catchFallPosInBS":
            {
                if (value is not Vector4[] castValue || castValue.Length != 2) return false;
                try
                {
                    _catchFallPosInBSInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_currentReachGain":
            case "currentReachGain":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _currentReachGainInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_timeSinceLastModify":
            case "timeSinceLastModify":
            {
                if (value is not float castValue) return false;
                instance.m_timeSinceLastModify = castValue;
                return true;
            }
            case "m_currentTwistGain":
            case "currentTwistGain":
            {
                if (value is not float castValue) return false;
                instance.m_currentTwistGain = castValue;
                return true;
            }
            case "m_currentTwistDirection":
            case "currentTwistDirection":
            {
                if (value is not short castValue) return false;
                instance.m_currentTwistDirection = castValue;
                return true;
            }
            case "m_catchFallPosIsValid":
            case "catchFallPosIsValid":
            {
                if (value is not bool[] castValue || castValue.Length != 2) return false;
                try
                {
                    _catchFallPosIsValidInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
