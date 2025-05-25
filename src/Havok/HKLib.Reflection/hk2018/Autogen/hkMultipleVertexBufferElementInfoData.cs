// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMultipleVertexBufferElementInfoData : HavokData<hkMultipleVertexBuffer.ElementInfo> 
{
    public hkMultipleVertexBufferElementInfoData(HavokType type, hkMultipleVertexBuffer.ElementInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBufferIndex":
            case "vertexBufferIndex":
            {
                if (instance.m_vertexBufferIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_elementIndex":
            case "elementIndex":
            {
                if (instance.m_elementIndex is not TGet castValue) return false;
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
            case "m_vertexBufferIndex":
            case "vertexBufferIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_vertexBufferIndex = castValue;
                return true;
            }
            case "m_elementIndex":
            case "elementIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_elementIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
