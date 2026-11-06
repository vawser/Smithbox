// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclClothStateBufferAccessData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclClothState.BufferAccess>
{
    public hclClothStateBufferAccessData(Havok.hk2018.hclClothState.BufferAccess instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bufferIndex":
            case "bufferIndex":
            {
                value = instance.m_bufferIndex;
                return true;
            }
            case "m_bufferUsage":
            case "bufferUsage":
            {
                value = instance.m_bufferUsage;
                return true;
            }
            case "m_shadowBufferIndex":
            case "shadowBufferIndex":
            {
                value = instance.m_shadowBufferIndex;
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
                if (value is not Havok.hk2018.hclBufferUsage castValue) return false;
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
