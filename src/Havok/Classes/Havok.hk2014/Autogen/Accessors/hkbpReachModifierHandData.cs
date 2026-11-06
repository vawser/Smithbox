// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpReachModifierHandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpReachModifierHand>
{
    public hkbpReachModifierHandData(Havok.hk2014.hkbpReachModifierHand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_targetOrSensingPosition":
            case "targetOrSensingPosition":
            {
                value = instance.m_targetOrSensingPosition;
                return true;
            }
            case "m_targetBackHandNormal":
            case "targetBackHandNormal":
            {
                value = instance.m_targetBackHandNormal;
                return true;
            }
            case "m_sensingRadius":
            case "sensingRadius":
            {
                value = instance.m_sensingRadius;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                value = instance.m_boneIndex;
                return true;
            }
            case "m_handIkTrackIndex":
            case "handIkTrackIndex":
            {
                value = instance.m_handIkTrackIndex;
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
            case "m_targetOrSensingPosition":
            case "targetOrSensingPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetOrSensingPosition = castValue;
                return true;
            }
            case "m_targetBackHandNormal":
            case "targetBackHandNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetBackHandNormal = castValue;
                return true;
            }
            case "m_sensingRadius":
            case "sensingRadius":
            {
                if (value is not float castValue) return false;
                instance.m_sensingRadius = castValue;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            case "m_handIkTrackIndex":
            case "handIkTrackIndex":
            {
                if (value is not short castValue) return false;
                instance.m_handIkTrackIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
