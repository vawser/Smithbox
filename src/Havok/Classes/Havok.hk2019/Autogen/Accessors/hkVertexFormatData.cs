// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkVertexFormatData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkVertexFormat>
{
    private static readonly System.Reflection.FieldInfo _elementsInfo = typeof(Havok.hk2019.hkVertexFormat).GetField("m_elements")!;
    public hkVertexFormatData(Havok.hk2019.hkVertexFormat instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                value = instance.m_elements;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                value = instance.m_numElements;
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
            case "m_elements":
            case "elements":
            {
                if (value is not Havok.hk2019.hkVertexFormat.Element[] castValue || castValue.Length != 32) return false;
                try
                {
                    _elementsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numElements":
            case "numElements":
            {
                if (value is not int castValue) return false;
                instance.m_numElements = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
