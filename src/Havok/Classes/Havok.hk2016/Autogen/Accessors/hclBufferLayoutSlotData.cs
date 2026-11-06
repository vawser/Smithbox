// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclBufferLayoutSlotData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclBufferLayout.Slot>
{
    public hclBufferLayoutSlotData(Havok.hk2016.hclBufferLayout.Slot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_stride":
            case "stride":
            {
                value = instance.m_stride;
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
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2016.hclBufferLayout.SlotFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2016.hclBufferLayout.SlotFlags)byteValue;
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
