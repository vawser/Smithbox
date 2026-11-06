// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkMultipleVertexBufferData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkMultipleVertexBuffer>
{
    public hkMultipleVertexBufferData(Havok.hk2014.hkMultipleVertexBuffer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexFormat":
            case "vertexFormat":
            {
                value = instance.m_vertexFormat;
                return true;
            }
            case "m_lockedElements":
            case "lockedElements":
            {
                value = instance.m_lockedElements;
                return true;
            }
            case "m_lockedBuffer":
            case "lockedBuffer":
            {
                value = instance.m_lockedBuffer;
                return true;
            }
            case "m_elementInfos":
            case "elementInfos":
            {
                value = instance.m_elementInfos;
                return true;
            }
            case "m_vertexBufferInfos":
            case "vertexBufferInfos":
            {
                value = instance.m_vertexBufferInfos;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
                return true;
            }
            case "m_isLocked":
            case "isLocked":
            {
                value = instance.m_isLocked;
                return true;
            }
            case "m_updateCount":
            case "updateCount":
            {
                value = instance.m_updateCount;
                return true;
            }
            case "m_writeLock":
            case "writeLock":
            {
                value = instance.m_writeLock;
                return true;
            }
            case "m_isSharable":
            case "isSharable":
            {
                value = instance.m_isSharable;
                return true;
            }
            case "m_constructionComplete":
            case "constructionComplete":
            {
                value = instance.m_constructionComplete;
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
            case "m_vertexFormat":
            case "vertexFormat":
            {
                if (value is not Havok.hk2014.hkVertexFormat castValue) return false;
                instance.m_vertexFormat = castValue;
                return true;
            }
            case "m_lockedElements":
            case "lockedElements":
            {
                if (value is not List<Havok.hk2014.hkMultipleVertexBuffer.LockedElement> castValue) return false;
                instance.m_lockedElements = castValue;
                return true;
            }
            case "m_lockedBuffer":
            case "lockedBuffer":
            {
                if (value is null)
                {
                    instance.m_lockedBuffer = default;
                    return true;
                }
                if (value is Havok.hk2014.hkMemoryMeshVertexBuffer castValue)
                {
                    instance.m_lockedBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_elementInfos":
            case "elementInfos":
            {
                if (value is not List<Havok.hk2014.hkMultipleVertexBuffer.ElementInfo> castValue) return false;
                instance.m_elementInfos = castValue;
                return true;
            }
            case "m_vertexBufferInfos":
            case "vertexBufferInfos":
            {
                if (value is not List<Havok.hk2014.hkMultipleVertexBuffer.VertexBufferInfo> castValue) return false;
                instance.m_vertexBufferInfos = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not int castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            case "m_isLocked":
            case "isLocked":
            {
                if (value is not bool castValue) return false;
                instance.m_isLocked = castValue;
                return true;
            }
            case "m_updateCount":
            case "updateCount":
            {
                if (value is not uint castValue) return false;
                instance.m_updateCount = castValue;
                return true;
            }
            case "m_writeLock":
            case "writeLock":
            {
                if (value is not bool castValue) return false;
                instance.m_writeLock = castValue;
                return true;
            }
            case "m_isSharable":
            case "isSharable":
            {
                if (value is not bool castValue) return false;
                instance.m_isSharable = castValue;
                return true;
            }
            case "m_constructionComplete":
            case "constructionComplete":
            {
                if (value is not bool castValue) return false;
                instance.m_constructionComplete = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
