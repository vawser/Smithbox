// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxMeshSectionInstanceInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxMeshSection.InstanceInfo>
{
    public hkxMeshSectionInstanceInfoData(Havok.hk2019.hkxMeshSection.InstanceInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localTransform":
            case "localTransform":
            {
                value = instance.m_localTransform;
                return true;
            }
            case "m_vertexBase":
            case "vertexBase":
            {
                value = instance.m_vertexBase;
                return true;
            }
            case "m_verticesCount":
            case "verticesCount":
            {
                value = instance.m_verticesCount;
                return true;
            }
            case "m_indexOffset":
            case "indexOffset":
            {
                value = instance.m_indexOffset;
                return true;
            }
            case "m_indicesCount":
            case "indicesCount":
            {
                value = instance.m_indicesCount;
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
            case "m_localTransform":
            case "localTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_localTransform = castValue;
                return true;
            }
            case "m_vertexBase":
            case "vertexBase":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexBase = castValue;
                return true;
            }
            case "m_verticesCount":
            case "verticesCount":
            {
                if (value is not uint castValue) return false;
                instance.m_verticesCount = castValue;
                return true;
            }
            case "m_indexOffset":
            case "indexOffset":
            {
                if (value is not uint castValue) return false;
                instance.m_indexOffset = castValue;
                return true;
            }
            case "m_indicesCount":
            case "indicesCount":
            {
                if (value is not uint castValue) return false;
                instance.m_indicesCount = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
