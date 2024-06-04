// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBufferLayoutData : HavokData<hclBufferLayout> 
{
    private static readonly System.Reflection.FieldInfo _elementsLayoutInfo = typeof(hclBufferLayout).GetField("m_elementsLayout")!;
    private static readonly System.Reflection.FieldInfo _slotsInfo = typeof(hclBufferLayout).GetField("m_slots")!;
    public hclBufferLayoutData(HavokType type, hclBufferLayout instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elementsLayout":
            case "elementsLayout":
            {
                if (instance.m_elementsLayout is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_slots":
            case "slots":
            {
                if (instance.m_slots is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numSlots":
            case "numSlots":
            {
                if (instance.m_numSlots is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFormat":
            case "triangleFormat":
            {
                if (instance.m_triangleFormat is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_triangleFormat is TGet byteValue)
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
            case "m_elementsLayout":
            case "elementsLayout":
            {
                if (value is not hclBufferLayout.BufferElement[] castValue || castValue.Length != 4) return false;
                try
                {
                    _elementsLayoutInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_slots":
            case "slots":
            {
                if (value is not hclBufferLayout.Slot[] castValue || castValue.Length != 4) return false;
                try
                {
                    _slotsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numSlots":
            case "numSlots":
            {
                if (value is not byte castValue) return false;
                instance.m_numSlots = castValue;
                return true;
            }
            case "m_triangleFormat":
            case "triangleFormat":
            {
                if (value is hclBufferLayout.TriangleFormat castValue)
                {
                    instance.m_triangleFormat = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_triangleFormat = (hclBufferLayout.TriangleFormat)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
