// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkVertexFormatData : HavokData<hkVertexFormat> 
{
    private static readonly System.Reflection.FieldInfo _elementsInfo = typeof(hkVertexFormat).GetField("m_elements")!;
    public hkVertexFormatData(HavokType type, hkVertexFormat instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                if (instance.m_elements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                if (instance.m_numElements is not TGet castValue) return false;
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
            case "m_elements":
            case "elements":
            {
                if (value is not hkVertexFormat.Element[] castValue || castValue.Length != 32) return false;
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
