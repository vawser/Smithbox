// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hknpMotionType;
using HKLib.hk2018.hknpWorld;
using Enum = HKLib.hk2018.hknpMotionType.Enum;

namespace HKLib.Reflection.hk2018;

internal class hknpSetBodyMotionTypeCommandData : HavokData<hknpSetBodyMotionTypeCommand> 
{
    public hknpSetBodyMotionTypeCommandData(HavokType type, hknpSetBodyMotionTypeCommand instance) : base(type, instance) {}

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
            case "m_bodyId":
            case "bodyId":
            {
                if (instance.m_bodyId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionType":
            case "motionType":
            {
                if (instance.m_motionType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_motionType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_cacheBehavior":
            case "cacheBehavior":
            {
                if (instance.m_cacheBehavior is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_cacheBehavior is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
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
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            case "m_motionType":
            case "motionType":
            {
                if (value is Enum castValue)
                {
                    instance.m_motionType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_motionType = (Enum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_cacheBehavior":
            case "cacheBehavior":
            {
                if (value is UpdateCachesMode castValue)
                {
                    instance.m_cacheBehavior = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_cacheBehavior = (UpdateCachesMode)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
