// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkbBodyIkControlBits;
using Enum = HKLib.hk2018.hkbBodyIkControlBits.Enum;

namespace HKLib.Reflection.hk2018;

internal class hkbBodyIkControlsModifierControlDataData : HavokData<hkbBodyIkControlsModifier.ControlData> 
{
    public hkbBodyIkControlsModifierControlDataData(HavokType type, hkbBodyIkControlsModifier.ControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                if (instance.m_fadeInDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (instance.m_fadeOutDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onEventId":
            case "onEventId":
            {
                if (instance.m_onEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offEventId":
            case "offEventId":
            {
                if (instance.m_offEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onByDefault":
            case "onByDefault":
            {
                if (instance.m_onByDefault is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forceFullFadeDurations":
            case "forceFullFadeDurations":
            {
                if (instance.m_forceFullFadeDurations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                if (instance.m_fadeInOutCurve is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_fadeInOutCurve is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_controlPointName":
            case "controlPointName":
            {
                if (instance.m_controlPointName is null)
                {
                    return true;
                }
                if (instance.m_controlPointName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_effectors":
            case "effectors":
            {
                if (instance.m_effectors is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_effectors is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_animationInfluence":
            case "animationInfluence":
            {
                if (instance.m_animationInfluence is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetPosition":
            case "targetPosition":
            {
                if (instance.m_targetPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetRotation":
            case "targetRotation":
            {
                if (instance.m_targetRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetHandle":
            case "targetHandle":
            {
                if (instance.m_targetHandle is null)
                {
                    return true;
                }
                if (instance.m_targetHandle is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_targetTransitionDuration":
            case "targetTransitionDuration":
            {
                if (instance.m_targetTransitionDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendAnimationDuringTargetTransition":
            case "blendAnimationDuringTargetTransition":
            {
                if (instance.m_blendAnimationDuringTargetTransition is not TGet castValue) return false;
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
                if (value is hkbBlendCurveUtils.BlendCurve castValue)
                {
                    instance.m_fadeInOutCurve = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadeInOutCurve = (hkbBlendCurveUtils.BlendCurve)sbyteValue;
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
                if (value is Enum castValue)
                {
                    instance.m_effectors = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_effectors = (Enum)ushortValue;
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
                if (value is hkbHandle castValue)
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
