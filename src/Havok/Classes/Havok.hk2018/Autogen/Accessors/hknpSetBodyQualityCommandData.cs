// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpSetBodyQualityCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpSetBodyQualityCommand>
{
    public hknpSetBodyQualityCommandData(Havok.hk2018.hknpSetBodyQualityCommand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                value = instance.m_sizePaddedTo16;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                value = instance.m_filterBits;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                value = instance.m_primaryType;
                return true;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                value = instance.m_secondaryType;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                value = instance.m_bodyId;
                return true;
            }
            case "m_qualityId":
            case "qualityId":
            {
                value = instance.m_qualityId;
                return true;
            }
            case "m_collisionCacheBehavior":
            case "collisionCacheBehavior":
            {
                value = instance.m_collisionCacheBehavior;
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
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                if (value is not ushort castValue) return false;
                instance.m_sizePaddedTo16 = castValue;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                if (value is not byte castValue) return false;
                instance.m_filterBits = castValue;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                if (value is Havok.hk2018.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (Havok.hk2018.hkCommand.PrimaryType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                if (value is not ushort castValue) return false;
                instance.m_secondaryType = castValue;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            case "m_qualityId":
            case "qualityId":
            {
                if (value is not Havok.hk2018.hknpBodyQualityId castValue) return false;
                instance.m_qualityId = castValue;
                return true;
            }
            case "m_collisionCacheBehavior":
            case "collisionCacheBehavior":
            {
                if (value is Havok.hk2018.hknpWorld.UpdateCachesMode castValue)
                {
                    instance.m_collisionCacheBehavior = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_collisionCacheBehavior = (Havok.hk2018.hknpWorld.UpdateCachesMode)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
