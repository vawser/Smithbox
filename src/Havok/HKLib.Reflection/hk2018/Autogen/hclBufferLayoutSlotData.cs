// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBufferLayoutSlotData : HavokData<hclBufferLayout.Slot> 
{
    public hclBufferLayoutSlotData(HavokType type, hclBufferLayout.Slot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_stride":
            case "stride":
            {
                if (instance.m_stride is not TGet castValue) return false;
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
            case "m_flags":
            case "flags":
            {
                if (value is hclBufferLayout.SlotFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (hclBufferLayout.SlotFlags)byteValue;
                    return true;
                }
                return false;
            }
            case "m_stride":
            case "stride":
            {
                if (value is not byte castValue) return false;
                instance.m_stride = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
