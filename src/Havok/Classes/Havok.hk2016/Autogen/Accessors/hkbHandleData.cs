// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbHandleData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbHandle>
{
    public hkbHandleData(Havok.hk2016.hkbHandle instance) : base(instance) { }

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
            case "m_frame":
            case "frame":
            {
                value = instance.m_frame;
                return true;
            }
            case "m_rigidBody":
            case "rigidBody":
            {
                value = instance.m_rigidBody;
                return true;
            }
            case "m_character":
            case "character":
            {
                value = instance.m_character;
                return true;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                value = instance.m_animationBoneIndex;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2016.hkLocalFrame castValue)
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
                if (value is Havok.hk2016.hkReferencedObject castValue)
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
                if (value is Havok.hk2016.hkbCharacter castValue)
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
