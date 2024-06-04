// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbHandleData : HavokData<hkbHandle> 
{
    public hkbHandleData(HavokType type, hkbHandle instance) : base(type, instance) {}

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
            case "m_frame":
            case "frame":
            {
                if (instance.m_frame is null)
                {
                    return true;
                }
                if (instance.m_frame is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_rigidBody":
            case "rigidBody":
            {
                if (instance.m_rigidBody is null)
                {
                    return true;
                }
                if (instance.m_rigidBody is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_character":
            case "character":
            {
                if (instance.m_character is null)
                {
                    return true;
                }
                if (instance.m_character is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                if (instance.m_animationBoneIndex is not TGet castValue) return false;
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
            case "m_frame":
            case "frame":
            {
                if (value is null)
                {
                    instance.m_frame = default;
                    return true;
                }
                if (value is hkLocalFrame castValue)
                {
                    instance.m_frame = castValue;
                    return true;
                }
                return false;
            }
            case "m_rigidBody":
            case "rigidBody":
            {
                if (value is null)
                {
                    instance.m_rigidBody = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_rigidBody = castValue;
                    return true;
                }
                return false;
            }
            case "m_character":
            case "character":
            {
                if (value is null)
                {
                    instance.m_character = default;
                    return true;
                }
                if (value is hkbCharacter castValue)
                {
                    instance.m_character = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_animationBoneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
