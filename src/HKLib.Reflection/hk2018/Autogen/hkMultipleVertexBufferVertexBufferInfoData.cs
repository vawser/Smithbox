// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMultipleVertexBufferVertexBufferInfoData : HavokData<hkMultipleVertexBuffer.VertexBufferInfo> 
{
    public hkMultipleVertexBufferVertexBufferInfoData(HavokType type, hkMultipleVertexBuffer.VertexBufferInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (instance.m_vertexBuffer is null)
                {
                    return true;
                }
                if (instance.m_vertexBuffer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_isLocked":
            case "isLocked":
            {
                if (instance.m_isLocked is not TGet castValue) return false;
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
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (value is null)
                {
                    instance.m_vertexBuffer = default;
                    return true;
                }
                if (value is hkMeshVertexBuffer castValue)
                {
                    instance.m_vertexBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_isLocked":
            case "isLocked":
            {
                if (value is not bool castValue) return false;
                instance.m_isLocked = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
