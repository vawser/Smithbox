// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkMultipleVertexBufferVertexBufferInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkMultipleVertexBuffer.VertexBufferInfo>
{
    public hkMultipleVertexBufferVertexBufferInfoData(Havok.hk2014.hkMultipleVertexBuffer.VertexBufferInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                value = instance.m_vertexBuffer;
                return true;
            }
            case "m_isLocked":
            case "isLocked":
            {
                value = instance.m_isLocked;
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
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (value is null)
                {
                    instance.m_vertexBuffer = default;
                    return true;
                }
                if (value is Havok.hk2014.hkMeshVertexBuffer castValue)
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
