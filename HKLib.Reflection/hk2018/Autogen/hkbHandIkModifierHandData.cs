// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbHandIkModifierHandData : HavokData<hkbHandIkModifier.Hand> 
{
    public hkbHandIkModifierHandData(HavokType type, hkbHandIkModifier.Hand instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elbowAxisLS":
            case "elbowAxisLS":
            {
                if (instance.m_elbowAxisLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_backHandNormalLS":
            case "backHandNormalLS":
            {
                if (instance.m_backHandNormalLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handOffsetLS":
            case "handOffsetLS":
            {
                if (instance.m_handOffsetLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handOrientationOffsetLS":
            case "handOrientationOffsetLS":
            {
                if (instance.m_handOrientationOffsetLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxElbowAngleDegrees":
            case "maxElbowAngleDegrees":
            {
                if (instance.m_maxElbowAngleDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minElbowAngleDegrees":
            case "minElbowAngleDegrees":
            {
                if (instance.m_minElbowAngleDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shoulderIndex":
            case "shoulderIndex":
            {
                if (instance.m_shoulderIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shoulderSiblingIndex":
            case "shoulderSiblingIndex":
            {
                if (instance.m_shoulderSiblingIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_elbowIndex":
            case "elbowIndex":
            {
                if (instance.m_elbowIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_elbowSiblingIndex":
            case "elbowSiblingIndex":
            {
                if (instance.m_elbowSiblingIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wristIndex":
            case "wristIndex":
            {
                if (instance.m_wristIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enforceEndPosition":
            case "enforceEndPosition":
            {
                if (instance.m_enforceEndPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enforceEndRotation":
            case "enforceEndRotation":
            {
                if (instance.m_enforceEndRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                if (instance.m_localFrameName is null)
                {
                    return true;
                }
                if (instance.m_localFrameName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
            case "m_handOrientationOffsetLS":
            case "handOrientationOffsetLS":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_handOrientationOffsetLS = castValue;
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
