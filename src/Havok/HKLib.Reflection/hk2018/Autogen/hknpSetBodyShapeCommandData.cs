// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpSetBodyShapeCommandData : HavokData<hknpSetBodyShapeCommand> 
{
    public hknpSetBodyShapeCommandData(HavokType type, hknpSetBodyShapeCommand instance) : base(type, instance) {}

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
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
