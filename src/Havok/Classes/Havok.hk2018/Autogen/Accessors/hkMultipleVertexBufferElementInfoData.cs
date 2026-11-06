// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMultipleVertexBufferElementInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMultipleVertexBuffer.ElementInfo>
{
    public hkMultipleVertexBufferElementInfoData(Havok.hk2018.hkMultipleVertexBuffer.ElementInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBufferIndex":
            case "vertexBufferIndex":
            {
                value = instance.m_vertexBufferIndex;
                return true;
            }
            case "m_elementIndex":
            case "elementIndex":
            {
                value = instance.m_elementIndex;
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
