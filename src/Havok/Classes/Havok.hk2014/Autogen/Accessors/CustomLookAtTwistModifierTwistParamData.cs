// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class CustomLookAtTwistModifierTwistParamData : Havok.Formats.HavokFieldAccess<Havok.hk2014.CustomLookAtTwistModifier.TwistParam>
{
    public CustomLookAtTwistModifierTwistParamData(Havok.hk2014.CustomLookAtTwistModifier.TwistParam instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                value = instance.m_startBoneIndex;
                return true;
            }
            case "m_endBoneIndex":
            case "endBoneIndex":
            {
                value = instance.m_endBoneIndex;
                return true;
            }
            case "m_targetRotationRate":
            case "targetRotationRate":
            {
                value = instance.m_targetRotationRate;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                value = instance.m_newTargetGain;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                value = instance.m_onGain;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                value = instance.m_offGain;
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
