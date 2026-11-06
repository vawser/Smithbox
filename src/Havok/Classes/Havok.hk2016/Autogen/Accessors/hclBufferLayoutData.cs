// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclBufferLayoutData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclBufferLayout>
{
    private static readonly System.Reflection.FieldInfo _elementsLayoutInfo = typeof(Havok.hk2016.hclBufferLayout).GetField("m_elementsLayout")!;
    private static readonly System.Reflection.FieldInfo _slotsInfo = typeof(Havok.hk2016.hclBufferLayout).GetField("m_slots")!;
    public hclBufferLayoutData(Havok.hk2016.hclBufferLayout instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elementsLayout":
            case "elementsLayout":
            {
                value = instance.m_elementsLayout;
                return true;
            }
            case "m_slots":
            case "slots":
            {
                value = instance.m_slots;
                return true;
            }
            case "m_numSlots":
            case "numSlots":
            {
                value = instance.m_numSlots;
                return true;
            }
            case "m_triangleFormat":
            case "triangleFormat":
            {
                value = instance.m_triangleFormat;
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
            case "m_elementsLayout":
            case "elementsLayout":
            {
                if (value is not Havok.hk2016.hclBufferLayout.BufferElement[] castValue || castValue.Length != 4) return false;
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
                if (value is not Havok.hk2016.hclBufferLayout.Slot[] castValue || castValue.Length != 4) return false;
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
                if (value is Havok.hk2016.hclBufferLayout.TriangleFormat castValue)
                {
                    instance.m_triangleFormat = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_triangleFormat = (Havok.hk2016.hclBufferLayout.TriangleFormat)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
