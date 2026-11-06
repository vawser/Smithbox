// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbKeyframeBonesModifierKeyframeInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbKeyframeBonesModifier.KeyframeInfo>
{
    public hkbKeyframeBonesModifierKeyframeInfoData(Havok.hk2018.hkbKeyframeBonesModifier.KeyframeInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_keyframedPosition":
            case "keyframedPosition":
            {
                value = instance.m_keyframedPosition;
                return true;
            }
            case "m_keyframedRotation":
            case "keyframedRotation":
            {
                value = instance.m_keyframedRotation;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                value = instance.m_boneIndex;
                return true;
            }
            case "m_isValid":
            case "isValid":
            {
                value = instance.m_isValid;
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
