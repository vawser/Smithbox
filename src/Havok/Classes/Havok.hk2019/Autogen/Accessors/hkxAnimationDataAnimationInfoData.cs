// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAnimationDataAnimationInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAnimationData.AnimationInfo>
{
    public hkxAnimationDataAnimationInfoData(Havok.hk2019.hkxAnimationData.AnimationInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startTime":
            case "startTime":
            {
                value = instance.m_startTime;
                return true;
            }
            case "m_endTime":
            case "endTime":
            {
                value = instance.m_endTime;
                return true;
            }
            case "m_framesPerSecond":
            case "framesPerSecond":
            {
                value = instance.m_framesPerSecond;
                return true;
            }
            case "m_rotationOrder":
            case "rotationOrder":
            {
                value = instance.m_rotationOrder;
                return true;
            }
            case "m_preRotation":
            case "preRotation":
            {
                value = instance.m_preRotation;
                return true;
            }
            case "m_postRotation":
            case "postRotation":
            {
                value = instance.m_postRotation;
                return true;
            }
            case "m_postTransform":
            case "postTransform":
            {
                value = instance.m_postTransform;
                return true;
            }
            case "m_postTransformInverse":
            case "postTransformInverse":
            {
                value = instance.m_postTransformInverse;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                value = instance.m_bindPose;
                return true;
            }
            case "m_rotationOffset":
            case "rotationOffset":
            {
                value = instance.m_rotationOffset;
                return true;
            }
            case "m_rotationPivot":
            case "rotationPivot":
            {
                value = instance.m_rotationPivot;
                return true;
            }
            case "m_scalingOffset":
            case "scalingOffset":
            {
                value = instance.m_scalingOffset;
                return true;
            }
            case "m_scalingPivot":
            case "scalingPivot":
            {
                value = instance.m_scalingPivot;
                return true;
            }
            case "m_geometricT":
            case "geometricT":
            {
                value = instance.m_geometricT;
                return true;
            }
            case "m_geometricR":
            case "geometricR":
            {
                value = instance.m_geometricR;
                return true;
            }
            case "m_geometricS":
            case "geometricS":
            {
                value = instance.m_geometricS;
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
                if (value is not Havok.hk2019.hkQsTransform castValue) return false;
                instance.m_postTransform = castValue;
                return true;
            }
            case "m_postTransformInverse":
            case "postTransformInverse":
            {
                if (value is not Havok.hk2019.hkQsTransform castValue) return false;
                instance.m_postTransformInverse = castValue;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                if (value is not Havok.hk2019.hkQsTransform castValue) return false;
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
