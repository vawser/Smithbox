// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBlendingTransitionEffectInternalStateData : HavokData<hkbBlendingTransitionEffectInternalState> 
{
    public hkbBlendingTransitionEffectInternalStateData(HavokType type, hkbBlendingTransitionEffectInternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fromPos":
            case "fromPos":
            {
                if (instance.m_fromPos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fromRot":
            case "fromRot":
            {
                if (instance.m_fromRot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toPos":
            case "toPos":
            {
                if (instance.m_toPos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toRot":
            case "toRot":
            {
                if (instance.m_toRot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastPos":
            case "lastPos":
            {
                if (instance.m_lastPos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastRot":
            case "lastRot":
            {
                if (instance.m_lastRot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterPoseAtBeginningOfTransition":
            case "characterPoseAtBeginningOfTransition":
            {
                if (instance.m_characterPoseAtBeginningOfTransition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeRemaining":
            case "timeRemaining":
            {
                if (instance.m_timeRemaining is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeInTransition":
            case "timeInTransition":
            {
                if (instance.m_timeInTransition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toGeneratorSelfTranstitionMode":
            case "toGeneratorSelfTranstitionMode":
            {
                if (instance.m_toGeneratorSelfTranstitionMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_toGeneratorSelfTranstitionMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_initializeCharacterPose":
            case "initializeCharacterPose":
            {
                if (instance.m_initializeCharacterPose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignThisFrame":
            case "alignThisFrame":
            {
                if (instance.m_alignThisFrame is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignmentFinished":
            case "alignmentFinished":
            {
                if (instance.m_alignmentFinished is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is not List<hkQsTransform> castValue) return false;
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
                if (value is hkbTransitionEffect.SelfTransitionMode castValue)
                {
                    instance.m_toGeneratorSelfTranstitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_toGeneratorSelfTranstitionMode = (hkbTransitionEffect.SelfTransitionMode)sbyteValue;
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
