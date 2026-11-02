// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxMeshSectionInstanceInfoData : HavokData<HKLib.hk2019.hkxMeshSection.InstanceInfo> 
{
    public hkxMeshSectionInstanceInfoData(HavokType type, HKLib.hk2019.hkxMeshSection.InstanceInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localTransform":
            case "localTransform":
            {
                if (instance.m_localTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexBase":
            case "vertexBase":
            {
                if (instance.m_vertexBase is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_verticesCount":
            case "verticesCount":
            {
                if (instance.m_verticesCount is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexOffset":
            case "indexOffset":
            {
                if (instance.m_indexOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indicesCount":
            case "indicesCount":
            {
                if (instance.m_indicesCount is not TGet castValue) return false;
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
