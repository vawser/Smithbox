// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpFullCastCollisionEventData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpFullCastCollisionEvent>
{
    public hknpFullCastCollisionEventData(Havok.hk2019.hknpFullCastCollisionEvent instance) : base(instance) { }

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
            case "m_bodyIds":
            case "bodyIds":
            {
                value = instance.m_bodyIds;
                return true;
            }
            case "m_contactPosition":
            case "contactPosition":
            {
                value = instance.m_contactPosition;
                return true;
            }
            case "m_contactNormalAndDistance":
            case "contactNormalAndDistance":
            {
                value = instance.m_contactNormalAndDistance;
                return true;
            }
            case "m_motionTransformA":
            case "motionTransformA":
            {
                value = instance.m_motionTransformA;
                return true;
            }
            case "m_linearVelocityA":
            case "linearVelocityA":
            {
                value = instance.m_linearVelocityA;
                return true;
            }
            case "m_angularVelocityA":
            case "angularVelocityA":
            {
                value = instance.m_angularVelocityA;
                return true;
            }
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_shapeKeyA":
            case "shapeKeyA":
            {
                value = instance.m_shapeKeyA;
                return true;
            }
            case "m_shapeKeyB":
            case "shapeKeyB":
            {
                value = instance.m_shapeKeyB;
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
                if (value is Havok.hk2019.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (Havok.hk2019.hkCommand.PrimaryType)byteValue;
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
            case "m_bodyIds":
            case "bodyIds":
            {
                if (value is not Havok.hk2019.hknpBodyIdPair castValue) return false;
                instance.m_bodyIds = castValue;
                return true;
            }
            case "m_contactPosition":
            case "contactPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_contactPosition = castValue;
                return true;
            }
            case "m_contactNormalAndDistance":
            case "contactNormalAndDistance":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_contactNormalAndDistance = castValue;
                return true;
            }
            case "m_motionTransformA":
            case "motionTransformA":
            {
                if (value is not Havok.hk2019.hkQTransform castValue) return false;
                instance.m_motionTransformA = castValue;
                return true;
            }
            case "m_linearVelocityA":
            case "linearVelocityA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocityA = castValue;
                return true;
            }
            case "m_angularVelocityA":
            case "angularVelocityA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocityA = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_shapeKeyA":
            case "shapeKeyA":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_shapeKeyA = castValue;
                return true;
            }
            case "m_shapeKeyB":
            case "shapeKeyB":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_shapeKeyB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
