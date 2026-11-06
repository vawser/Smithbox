// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBodyIkControlsModifierControlDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBodyIkControlsModifier.ControlData>
{
    public hkbBodyIkControlsModifierControlDataData(Havok.hk2018.hkbBodyIkControlsModifier.ControlData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                value = instance.m_fadeInDuration;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                value = instance.m_fadeOutDuration;
                return true;
            }
            case "m_onEventId":
            case "onEventId":
            {
                value = instance.m_onEventId;
                return true;
            }
            case "m_offEventId":
            case "offEventId":
            {
                value = instance.m_offEventId;
                return true;
            }
            case "m_onByDefault":
            case "onByDefault":
            {
                value = instance.m_onByDefault;
                return true;
            }
            case "m_forceFullFadeDurations":
            case "forceFullFadeDurations":
            {
                value = instance.m_forceFullFadeDurations;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                value = instance.m_fadeInOutCurve;
                return true;
            }
            case "m_controlPointName":
            case "controlPointName":
            {
                value = instance.m_controlPointName;
                return true;
            }
            case "m_effectors":
            case "effectors":
            {
                value = instance.m_effectors;
                return true;
            }
            case "m_animationInfluence":
            case "animationInfluence":
            {
                value = instance.m_animationInfluence;
                return true;
            }
            case "m_targetPosition":
            case "targetPosition":
            {
                value = instance.m_targetPosition;
                return true;
            }
            case "m_targetRotation":
            case "targetRotation":
            {
                value = instance.m_targetRotation;
                return true;
            }
            case "m_targetHandle":
            case "targetHandle":
            {
                value = instance.m_targetHandle;
                return true;
            }
            case "m_targetTransitionDuration":
            case "targetTransitionDuration":
            {
                value = instance.m_targetTransitionDuration;
                return true;
            }
            case "m_blendAnimationDuringTargetTransition":
            case "blendAnimationDuringTargetTransition":
            {
                value = instance.m_blendAnimationDuringTargetTransition;
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
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInDuration = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_onEventId":
            case "onEventId":
            {
                if (value is not int castValue) return false;
                instance.m_onEventId = castValue;
                return true;
            }
            case "m_offEventId":
            case "offEventId":
            {
                if (value is not int castValue) return false;
                instance.m_offEventId = castValue;
                return true;
            }
            case "m_onByDefault":
            case "onByDefault":
            {
                if (value is not bool castValue) return false;
                instance.m_onByDefault = castValue;
                return true;
            }
            case "m_forceFullFadeDurations":
            case "forceFullFadeDurations":
            {
                if (value is not bool castValue) return false;
                instance.m_forceFullFadeDurations = castValue;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                if (value is Havok.hk2018.hkbBlendCurveUtils.BlendCurve castValue)
                {
                    instance.m_fadeInOutCurve = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadeInOutCurve = (Havok.hk2018.hkbBlendCurveUtils.BlendCurve)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_controlPointName":
            case "controlPointName":
            {
                if (value is null)
                {
                    instance.m_controlPointName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_controlPointName = castValue;
                    return true;
                }
                return false;
            }
            case "m_effectors":
            case "effectors":
            {
                if (value is Havok.hk2018.hkbBodyIkControlBits.Enum castValue)
                {
                    instance.m_effectors = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_effectors = (Havok.hk2018.hkbBodyIkControlBits.Enum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_animationInfluence":
            case "animationInfluence":
            {
                if (value is not float castValue) return false;
                instance.m_animationInfluence = castValue;
                return true;
            }
            case "m_targetPosition":
            case "targetPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetPosition = castValue;
                return true;
            }
            case "m_targetRotation":
            case "targetRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_targetRotation = castValue;
                return true;
            }
            case "m_targetHandle":
            case "targetHandle":
            {
                if (value is null)
                {
                    instance.m_targetHandle = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbHandle castValue)
                {
                    instance.m_targetHandle = castValue;
                    return true;
                }
                return false;
            }
            case "m_targetTransitionDuration":
            case "targetTransitionDuration":
            {
                if (value is not float castValue) return false;
                instance.m_targetTransitionDuration = castValue;
                return true;
            }
            case "m_blendAnimationDuringTargetTransition":
            case "blendAnimationDuringTargetTransition":
            {
                if (value is not bool castValue) return false;
                instance.m_blendAnimationDuringTargetTransition = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
