// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMultipleVertexBufferData : HavokData<hkMultipleVertexBuffer> 
{
    public hkMultipleVertexBufferData(HavokType type, hkMultipleVertexBuffer instance) : base(type, instance) {}

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
            case "m_vertexFormat":
            case "vertexFormat":
            {
                if (instance.m_vertexFormat is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lockedElements":
            case "lockedElements":
            {
                if (instance.m_lockedElements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lockedBuffer":
            case "lockedBuffer":
            {
                if (instance.m_lockedBuffer is null)
                {
                    return true;
                }
                if (instance.m_lockedBuffer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_elementInfos":
            case "elementInfos":
            {
                if (instance.m_elementInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexBufferInfos":
            case "vertexBufferInfos":
            {
                if (instance.m_vertexBufferInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (instance.m_numVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isLocked":
            case "isLocked":
            {
                if (instance.m_isLocked is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateCount":
            case "updateCount":
            {
                if (instance.m_updateCount is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_writeLock":
            case "writeLock":
            {
                if (instance.m_writeLock is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isSharable":
            case "isSharable":
            {
                if (instance.m_isSharable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constructionComplete":
            case "constructionComplete":
            {
                if (instance.m_constructionComplete is not TGet castValue) return false;
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
            case "m_vertexFormat":
            case "vertexFormat":
            {
                if (value is not hkVertexFormat castValue) return false;
                instance.m_vertexFormat = castValue;
                return true;
            }
            case "m_lockedElements":
            case "lockedElements":
            {
                if (value is not List<hkMultipleVertexBuffer.LockedElement> castValue) return false;
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
                if (value is hkMemoryMeshVertexBuffer castValue)
                {
                    instance.m_lockedBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_elementInfos":
            case "elementInfos":
            {
                if (value is not List<hkMultipleVertexBuffer.ElementInfo> castValue) return false;
                instance.m_elementInfos = castValue;
                return true;
            }
            case "m_vertexBufferInfos":
            case "vertexBufferInfos":
            {
                if (value is not List<hkMultipleVertexBuffer.VertexBufferInfo> castValue) return false;
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
