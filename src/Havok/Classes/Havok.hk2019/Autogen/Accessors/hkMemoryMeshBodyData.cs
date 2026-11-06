// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMemoryMeshBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMemoryMeshBody>
{
    public hkMemoryMeshBodyData(Havok.hk2019.hkMemoryMeshBody instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_transformSet":
            case "transformSet":
            {
                value = instance.m_transformSet;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_vertexBuffers":
            case "vertexBuffers":
            {
                value = instance.m_vertexBuffers;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_transformSet":
            case "transformSet":
            {
                if (value is null)
                {
                    instance.m_transformSet = default;
                    return true;
                }
                if (value is Havok.hk2019.hkIndexedTransformSet castValue)
                {
                    instance.m_transformSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2019.hkMeshShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexBuffers":
            case "vertexBuffers":
            {
                if (value is not List<Havok.hk2019.hkMeshVertexBuffer?> castValue) return false;
                instance.m_vertexBuffers = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
