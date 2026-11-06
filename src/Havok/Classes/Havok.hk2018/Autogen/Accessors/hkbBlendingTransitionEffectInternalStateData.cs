// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBlendingTransitionEffectInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBlendingTransitionEffectInternalState>
{
    public hkbBlendingTransitionEffectInternalStateData(Havok.hk2018.hkbBlendingTransitionEffectInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_fromPos":
            case "fromPos":
            {
                value = instance.m_fromPos;
                return true;
            }
            case "m_fromRot":
            case "fromRot":
            {
                value = instance.m_fromRot;
                return true;
            }
            case "m_toPos":
            case "toPos":
            {
                value = instance.m_toPos;
                return true;
            }
            case "m_toRot":
            case "toRot":
            {
                value = instance.m_toRot;
                return true;
            }
            case "m_lastPos":
            case "lastPos":
            {
                value = instance.m_lastPos;
                return true;
            }
            case "m_lastRot":
            case "lastRot":
            {
                value = instance.m_lastRot;
                return true;
            }
            case "m_characterPoseAtBeginningOfTransition":
            case "characterPoseAtBeginningOfTransition":
            {
                value = instance.m_characterPoseAtBeginningOfTransition;
                return true;
            }
            case "m_timeRemaining":
            case "timeRemaining":
            {
                value = instance.m_timeRemaining;
                return true;
            }
            case "m_timeInTransition":
            case "timeInTransition":
            {
                value = instance.m_timeInTransition;
                return true;
            }
            case "m_toGeneratorSelfTranstitionMode":
            case "toGeneratorSelfTranstitionMode":
            {
                value = instance.m_toGeneratorSelfTranstitionMode;
                return true;
            }
            case "m_initializeCharacterPose":
            case "initializeCharacterPose":
            {
                value = instance.m_initializeCharacterPose;
                return true;
            }
            case "m_alignThisFrame":
            case "alignThisFrame":
            {
                value = instance.m_alignThisFrame;
                return true;
            }
            case "m_alignmentFinished":
            case "alignmentFinished":
            {
                value = instance.m_alignmentFinished;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_fromPos":
            case "fromPos":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_fromPos = castValue;
                return true;
            }
            case "m_fromRot":
            case "fromRot":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_fromRot = castValue;
                return true;
            }
            case "m_toPos":
            case "toPos":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_toPos = castValue;
                return true;
            }
            case "m_toRot":
            case "toRot":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_toRot = castValue;
                return true;
            }
            case "m_lastPos":
            case "lastPos":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lastPos = castValue;
                return true;
            }
            case "m_lastRot":
            case "lastRot":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_lastRot = castValue;
                return true;
            }
            case "m_characterPoseAtBeginningOfTransition":
            case "characterPoseAtBeginningOfTransition":
            {
                if (value is not List<Havok.hk2018.hkQsTransform> castValue) return false;
                instance.m_characterPoseAtBeginningOfTransition = castValue;
                return true;
            }
            case "m_timeRemaining":
            case "timeRemaining":
            {
                if (value is not float castValue) return false;
                instance.m_timeRemaining = castValue;
                return true;
            }
            case "m_timeInTransition":
            case "timeInTransition":
            {
                if (value is not float castValue) return false;
                instance.m_timeInTransition = castValue;
                return true;
            }
            case "m_toGeneratorSelfTranstitionMode":
            case "toGeneratorSelfTranstitionMode":
            {
                if (value is Havok.hk2018.hkbTransitionEffect.SelfTransitionMode castValue)
                {
                    instance.m_toGeneratorSelfTranstitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_toGeneratorSelfTranstitionMode = (Havok.hk2018.hkbTransitionEffect.SelfTransitionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_initializeCharacterPose":
            case "initializeCharacterPose":
            {
                if (value is not bool castValue) return false;
                instance.m_initializeCharacterPose = castValue;
                return true;
            }
            case "m_alignThisFrame":
            case "alignThisFrame":
            {
                if (value is not bool castValue) return false;
                instance.m_alignThisFrame = castValue;
                return true;
            }
            case "m_alignmentFinished":
            case "alignmentFinished":
            {
                if (value is not bool castValue) return false;
                instance.m_alignmentFinished = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
