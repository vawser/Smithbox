// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbKeyframeBonesModifierKeyframeInfoData : HavokData<hkbKeyframeBonesModifier.KeyframeInfo> 
{
    public hkbKeyframeBonesModifierKeyframeInfoData(HavokType type, hkbKeyframeBonesModifier.KeyframeInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_keyframedPosition":
            case "keyframedPosition":
            {
                if (instance.m_keyframedPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keyframedRotation":
            case "keyframedRotation":
            {
                if (instance.m_keyframedRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (instance.m_boneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isValid":
            case "isValid":
            {
                if (instance.m_isValid is not TGet castValue) return false;
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
            case "m_keyframedPosition":
            case "keyframedPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_keyframedPosition = castValue;
                return true;
            }
            case "m_keyframedRotation":
            case "keyframedRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_keyframedRotation = castValue;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            case "m_isValid":
            case "isValid":
            {
                if (value is not bool castValue) return false;
                instance.m_isValid = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
