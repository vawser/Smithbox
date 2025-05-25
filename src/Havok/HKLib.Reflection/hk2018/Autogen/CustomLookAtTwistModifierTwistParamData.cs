// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class CustomLookAtTwistModifierTwistParamData : HavokData<CustomLookAtTwistModifier.TwistParam> 
{
    public CustomLookAtTwistModifierTwistParamData(HavokType type, CustomLookAtTwistModifier.TwistParam instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                if (instance.m_startBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endBoneIndex":
            case "endBoneIndex":
            {
                if (instance.m_endBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetRotationRate":
            case "targetRotationRate":
            {
                if (instance.m_targetRotationRate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                if (instance.m_newTargetGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                if (instance.m_onGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                if (instance.m_offGain is not TGet castValue) return false;
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
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_startBoneIndex = castValue;
                return true;
            }
            case "m_endBoneIndex":
            case "endBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_endBoneIndex = castValue;
                return true;
            }
            case "m_targetRotationRate":
            case "targetRotationRate":
            {
                if (value is not float castValue) return false;
                instance.m_targetRotationRate = castValue;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                if (value is not float castValue) return false;
                instance.m_newTargetGain = castValue;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                if (value is not float castValue) return false;
                instance.m_onGain = castValue;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                if (value is not float castValue) return false;
                instance.m_offGain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
