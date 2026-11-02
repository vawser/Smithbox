// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiWorldCommandsEnableAsyncProcessingData : HavokData<HKLib.hk2019.hkaiWorldCommands.EnableAsyncProcessing> 
{
    public hkaiWorldCommandsEnableAsyncProcessingData(HavokType type, HKLib.hk2019.hkaiWorldCommands.EnableAsyncProcessing instance) : base(type, instance) {}

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
            case "m_asyncThreadPoolCinfo":
            case "asyncThreadPoolCinfo":
            {
                if (instance.m_asyncThreadPoolCinfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_automaticStepping":
            case "automaticStepping":
            {
                if (instance.m_automaticStepping is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (HKLib.hk2019.hkCommand.PrimaryType)byteValue;
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
            case "m_asyncThreadPoolCinfo":
            case "asyncThreadPoolCinfo":
            {
                if (value is not HKLib.hk2019.hkAsyncThreadPool.Cinfo castValue) return false;
                instance.m_asyncThreadPoolCinfo = castValue;
                return true;
            }
            case "m_automaticStepping":
            case "automaticStepping":
            {
                if (value is not bool castValue) return false;
                instance.m_automaticStepping = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
