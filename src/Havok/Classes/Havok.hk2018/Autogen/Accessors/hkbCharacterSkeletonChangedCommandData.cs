// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCharacterSkeletonChangedCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCharacterSkeletonChangedCommand>
{
    public hkbCharacterSkeletonChangedCommandData(Havok.hk2018.hkbCharacterSkeletonChangedCommand instance) : base(instance) { }

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
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                value = instance.m_skeleton;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                if (value is null)
                {
                    instance.m_skeleton = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not int castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
