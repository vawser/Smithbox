// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbHandIkDriverInfoHandData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbHandIkDriverInfo.Hand>
{
    public hkbHandIkDriverInfoHandData(Havok.hk2016.hkbHandIkDriverInfo.Hand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elbowAxisLS":
            case "elbowAxisLS":
            {
                value = instance.m_elbowAxisLS;
                return true;
            }
            case "m_backHandNormalLS":
            case "backHandNormalLS":
            {
                value = instance.m_backHandNormalLS;
                return true;
            }
            case "m_handOffsetLS":
            case "handOffsetLS":
            {
                value = instance.m_handOffsetLS;
                return true;
            }
            case "m_handOrienationOffsetLS":
            case "handOrienationOffsetLS":
            {
                value = instance.m_handOrienationOffsetLS;
                return true;
            }
            case "m_maxElbowAngleDegrees":
            case "maxElbowAngleDegrees":
            {
                value = instance.m_maxElbowAngleDegrees;
                return true;
            }
            case "m_minElbowAngleDegrees":
            case "minElbowAngleDegrees":
            {
                value = instance.m_minElbowAngleDegrees;
                return true;
            }
            case "m_shoulderIndex":
            case "shoulderIndex":
            {
                value = instance.m_shoulderIndex;
                return true;
            }
            case "m_shoulderSiblingIndex":
            case "shoulderSiblingIndex":
            {
                value = instance.m_shoulderSiblingIndex;
                return true;
            }
            case "m_elbowIndex":
            case "elbowIndex":
            {
                value = instance.m_elbowIndex;
                return true;
            }
            case "m_elbowSiblingIndex":
            case "elbowSiblingIndex":
            {
                value = instance.m_elbowSiblingIndex;
                return true;
            }
            case "m_wristIndex":
            case "wristIndex":
            {
                value = instance.m_wristIndex;
                return true;
            }
            case "m_enforceEndPosition":
            case "enforceEndPosition":
            {
                value = instance.m_enforceEndPosition;
                return true;
            }
            case "m_enforceEndRotation":
            case "enforceEndRotation":
            {
                value = instance.m_enforceEndRotation;
                return true;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                value = instance.m_localFrameName;
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
            case "m_elbowAxisLS":
            case "elbowAxisLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_elbowAxisLS = castValue;
                return true;
            }
            case "m_backHandNormalLS":
            case "backHandNormalLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_backHandNormalLS = castValue;
                return true;
            }
            case "m_handOffsetLS":
            case "handOffsetLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_handOffsetLS = castValue;
                return true;
            }
            case "m_handOrienationOffsetLS":
            case "handOrienationOffsetLS":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_handOrienationOffsetLS = castValue;
                return true;
            }
            case "m_maxElbowAngleDegrees":
            case "maxElbowAngleDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_maxElbowAngleDegrees = castValue;
                return true;
            }
            case "m_minElbowAngleDegrees":
            case "minElbowAngleDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_minElbowAngleDegrees = castValue;
                return true;
            }
            case "m_shoulderIndex":
            case "shoulderIndex":
            {
                if (value is not short castValue) return false;
                instance.m_shoulderIndex = castValue;
                return true;
            }
            case "m_shoulderSiblingIndex":
            case "shoulderSiblingIndex":
            {
                if (value is not short castValue) return false;
                instance.m_shoulderSiblingIndex = castValue;
                return true;
            }
            case "m_elbowIndex":
            case "elbowIndex":
            {
                if (value is not short castValue) return false;
                instance.m_elbowIndex = castValue;
                return true;
            }
            case "m_elbowSiblingIndex":
            case "elbowSiblingIndex":
            {
                if (value is not short castValue) return false;
                instance.m_elbowSiblingIndex = castValue;
                return true;
            }
            case "m_wristIndex":
            case "wristIndex":
            {
                if (value is not short castValue) return false;
                instance.m_wristIndex = castValue;
                return true;
            }
            case "m_enforceEndPosition":
            case "enforceEndPosition":
            {
                if (value is not bool castValue) return false;
                instance.m_enforceEndPosition = castValue;
                return true;
            }
            case "m_enforceEndRotation":
            case "enforceEndRotation":
            {
                if (value is not bool castValue) return false;
                instance.m_enforceEndRotation = castValue;
                return true;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                if (value is null)
                {
                    instance.m_localFrameName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_localFrameName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
