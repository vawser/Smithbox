// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimationDataAnimationInfoData : HavokData<HKLib.hk2019.hkxAnimationData.AnimationInfo> 
{
    public hkxAnimationDataAnimationInfoData(HavokType type, HKLib.hk2019.hkxAnimationData.AnimationInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startTime":
            case "startTime":
            {
                if (instance.m_startTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endTime":
            case "endTime":
            {
                if (instance.m_endTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_framesPerSecond":
            case "framesPerSecond":
            {
                if (instance.m_framesPerSecond is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationOrder":
            case "rotationOrder":
            {
                if (instance.m_rotationOrder is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_preRotation":
            case "preRotation":
            {
                if (instance.m_preRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_postRotation":
            case "postRotation":
            {
                if (instance.m_postRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_postTransform":
            case "postTransform":
            {
                if (instance.m_postTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_postTransformInverse":
            case "postTransformInverse":
            {
                if (instance.m_postTransformInverse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                if (instance.m_bindPose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationOffset":
            case "rotationOffset":
            {
                if (instance.m_rotationOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationPivot":
            case "rotationPivot":
            {
                if (instance.m_rotationPivot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scalingOffset":
            case "scalingOffset":
            {
                if (instance.m_scalingOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scalingPivot":
            case "scalingPivot":
            {
                if (instance.m_scalingPivot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometricT":
            case "geometricT":
            {
                if (instance.m_geometricT is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometricR":
            case "geometricR":
            {
                if (instance.m_geometricR is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometricS":
            case "geometricS":
            {
                if (instance.m_geometricS is not TGet castValue) return false;
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
            case "m_startTime":
            case "startTime":
            {
                if (value is not double castValue) return false;
                instance.m_startTime = castValue;
                return true;
            }
            case "m_endTime":
            case "endTime":
            {
                if (value is not double castValue) return false;
                instance.m_endTime = castValue;
                return true;
            }
            case "m_framesPerSecond":
            case "framesPerSecond":
            {
                if (value is not double castValue) return false;
                instance.m_framesPerSecond = castValue;
                return true;
            }
            case "m_rotationOrder":
            case "rotationOrder":
            {
                if (value is not int castValue) return false;
                instance.m_rotationOrder = castValue;
                return true;
            }
            case "m_preRotation":
            case "preRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_preRotation = castValue;
                return true;
            }
            case "m_postRotation":
            case "postRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_postRotation = castValue;
                return true;
            }
            case "m_postTransform":
            case "postTransform":
            {
                if (value is not HKLib.hk2019.hkQsTransform castValue) return false;
                instance.m_postTransform = castValue;
                return true;
            }
            case "m_postTransformInverse":
            case "postTransformInverse":
            {
                if (value is not HKLib.hk2019.hkQsTransform castValue) return false;
                instance.m_postTransformInverse = castValue;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                if (value is not HKLib.hk2019.hkQsTransform castValue) return false;
                instance.m_bindPose = castValue;
                return true;
            }
            case "m_rotationOffset":
            case "rotationOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rotationOffset = castValue;
                return true;
            }
            case "m_rotationPivot":
            case "rotationPivot":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rotationPivot = castValue;
                return true;
            }
            case "m_scalingOffset":
            case "scalingOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scalingOffset = castValue;
                return true;
            }
            case "m_scalingPivot":
            case "scalingPivot":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scalingPivot = castValue;
                return true;
            }
            case "m_geometricT":
            case "geometricT":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_geometricT = castValue;
                return true;
            }
            case "m_geometricR":
            case "geometricR":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_geometricR = castValue;
                return true;
            }
            case "m_geometricS":
            case "geometricS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_geometricS = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
