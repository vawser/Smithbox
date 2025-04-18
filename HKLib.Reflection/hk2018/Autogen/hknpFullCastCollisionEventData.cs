// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpFullCastCollisionEventData : HavokData<hknpFullCastCollisionEvent> 
{
    private static readonly System.Reflection.FieldInfo _bodyIdsInfo = typeof(hknpFullCastCollisionEvent).GetField("m_bodyIds")!;
    public hknpFullCastCollisionEventData(HavokType type, hknpFullCastCollisionEvent instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                if (instance.m_sizePaddedTo16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                if (instance.m_filterBits is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                if (instance.m_primaryType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_primaryType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                if (instance.m_secondaryType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIds":
            case "bodyIds":
            {
                if (instance.m_bodyIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactPosition":
            case "contactPosition":
            {
                if (instance.m_contactPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactNormalAndDistance":
            case "contactNormalAndDistance":
            {
                if (instance.m_contactNormalAndDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionTransformA":
            case "motionTransformA":
            {
                if (instance.m_motionTransformA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocityA":
            case "linearVelocityA":
            {
                if (instance.m_linearVelocityA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocityA":
            case "angularVelocityA":
            {
                if (instance.m_angularVelocityA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (instance.m_time is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeKeyA":
            case "shapeKeyA":
            {
                if (instance.m_shapeKeyA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeKeyB":
            case "shapeKeyB":
            {
                if (instance.m_shapeKeyB is not TGet castValue) return false;
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
                if (value is hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (hkCommand.PrimaryType)byteValue;
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
                if (value is not hknpBodyId[] castValue || castValue.Length != 2) return false;
                try
                {
                    _bodyIdsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
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
                if (value is not hkQTransform castValue) return false;
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
                if (value is not hkHandle<uint> castValue) return false;
                instance.m_shapeKeyA = castValue;
                return true;
            }
            case "m_shapeKeyB":
            case "shapeKeyB":
            {
                if (value is not hkHandle<uint> castValue) return false;
                instance.m_shapeKeyB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
