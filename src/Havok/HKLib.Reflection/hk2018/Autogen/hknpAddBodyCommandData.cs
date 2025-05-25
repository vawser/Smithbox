// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hknpWorld;

namespace HKLib.Reflection.hk2018;

internal class hknpAddBodyCommandData : HavokData<hknpAddBodyCommand> 
{
    public hknpAddBodyCommandData(HavokType type, hknpAddBodyCommand instance) : base(type, instance) {}

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
            case "m_additionMode":
            case "additionMode":
            {
                if (instance.m_additionMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_additionMode is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_activationMode":
            case "activationMode":
            {
                if (instance.m_activationMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_activationMode is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_isLastInBatch":
            case "isLastInBatch":
            {
                if (instance.m_isLastInBatch is not TGet castValue) return false;
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
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            case "m_additionMode":
            case "additionMode":
            {
                if (value is AdditionMode castValue)
                {
                    instance.m_additionMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_additionMode = (AdditionMode)byteValue;
                    return true;
                }
                return false;
            }
            case "m_activationMode":
            case "activationMode":
            {
                if (value is ActivationMode castValue)
                {
                    instance.m_activationMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_activationMode = (ActivationMode)byteValue;
                    return true;
                }
                return false;
            }
            case "m_isLastInBatch":
            case "isLastInBatch":
            {
                if (value is not bool castValue) return false;
                instance.m_isLastInBatch = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
