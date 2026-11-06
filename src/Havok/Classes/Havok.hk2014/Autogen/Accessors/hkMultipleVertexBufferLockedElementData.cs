// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkMultipleVertexBufferLockedElementData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkMultipleVertexBuffer.LockedElement>
{
    public hkMultipleVertexBufferLockedElementData(Havok.hk2014.hkMultipleVertexBuffer.LockedElement instance) : base(instance) { }

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
            case "m_lockedBufferIndex":
            case "lockedBufferIndex":
            {
                value = instance.m_lockedBufferIndex;
                return true;
            }
            case "m_vertexFormatIndex":
            case "vertexFormatIndex":
            {
                value = instance.m_vertexFormatIndex;
                return true;
            }
            case "m_lockFlags":
            case "lockFlags":
            {
                value = instance.m_lockFlags;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                value = instance.m_outputBufferIndex;
                return true;
            }
            case "m_emulatedIndex":
            case "emulatedIndex":
            {
                value = instance.m_emulatedIndex;
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
            case "m_lockedBufferIndex":
            case "lockedBufferIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_lockedBufferIndex = castValue;
                return true;
            }
            case "m_vertexFormatIndex":
            case "vertexFormatIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_vertexFormatIndex = castValue;
                return true;
            }
            case "m_lockFlags":
            case "lockFlags":
            {
                if (value is not byte castValue) return false;
                instance.m_lockFlags = castValue;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_outputBufferIndex = castValue;
                return true;
            }
            case "m_emulatedIndex":
            case "emulatedIndex":
            {
                if (value is not sbyte castValue) return false;
                instance.m_emulatedIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
