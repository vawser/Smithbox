// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxVertexBufferData : HavokData<HKLib.hk2019.hkxVertexBuffer> 
{
    public hkxVertexBufferData(HavokType type, HKLib.hk2019.hkxVertexBuffer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_data":
            case "data":
            {
                if (value is not HKLib.hk2019.hkxVertexBuffer.VertexData castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_desc":
            case "desc":
            {
                if (value is not HKLib.hk2019.hkxVertexDescription castValue) return false;
                instance.m_desc = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
