// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxVertexBufferData : HavokData<hkxVertexBuffer> 
{
    public hkxVertexBufferData(HavokType type, hkxVertexBuffer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desc":
            case "desc":
            {
                if (instance.m_desc is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not hkxVertexBuffer.VertexData castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_desc":
            case "desc":
            {
                if (value is not hkxVertexDescription castValue) return false;
                instance.m_desc = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
