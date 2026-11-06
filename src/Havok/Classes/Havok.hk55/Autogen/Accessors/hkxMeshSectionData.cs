// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxMeshSection>
{
    public hkxMeshSectionData(Havok.hk55.hkxMeshSection instance) : base(instance) { }

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
            case "m_indexBuffers":
            case "indexBuffers":
            {
                value = instance.m_indexBuffers;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_userChannels":
            case "userChannels":
            {
                value = instance.m_userChannels;
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
                if (value is Havok.hk55.hkxVertexBuffer castValue)
                {
                    instance.m_vertexBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexBuffers":
            case "indexBuffers":
            {
                if (value is not List<Havok.hk55.hkxIndexBuffer?> castValue) return false;
                instance.m_indexBuffers = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (value is null)
                {
                    instance.m_material = default;
                    return true;
                }
                if (value is Havok.hk55.hkxMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_userChannels":
            case "userChannels":
            {
                if (value is not List<Havok.hk55.hkVariant> castValue) return false;
                instance.m_userChannels = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
