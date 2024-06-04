// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothStateBufferAccessData : HavokData<hclClothState.BufferAccess> 
{
    public hclClothStateBufferAccessData(HavokType type, hclClothState.BufferAccess instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bufferIndex":
            case "bufferIndex":
            {
                if (instance.m_bufferIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferUsage":
            case "bufferUsage":
            {
                if (instance.m_bufferUsage is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shadowBufferIndex":
            case "shadowBufferIndex":
            {
                if (instance.m_shadowBufferIndex is not TGet castValue) return false;
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
            case "m_bufferIndex":
            case "bufferIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIndex = castValue;
                return true;
            }
            case "m_bufferUsage":
            case "bufferUsage":
            {
                if (value is not hclBufferUsage castValue) return false;
                instance.m_bufferUsage = castValue;
                return true;
            }
            case "m_shadowBufferIndex":
            case "shadowBufferIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_shadowBufferIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
