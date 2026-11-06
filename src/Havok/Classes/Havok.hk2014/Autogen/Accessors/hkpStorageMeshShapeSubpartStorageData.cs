// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpStorageMeshShapeSubpartStorageData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpStorageMeshShapeSubpartStorage>
{
    public hkpStorageMeshShapeSubpartStorageData(Havok.hk2014.hkpStorageMeshShapeSubpartStorage instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_indices16":
            case "indices16":
            {
                value = instance.m_indices16;
                return true;
            }
            case "m_indices32":
            case "indices32":
            {
                value = instance.m_indices32;
                return true;
            }
            case "m_materialIndices":
            case "materialIndices":
            {
                value = instance.m_materialIndices;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                value = instance.m_materials;
                return true;
            }
            case "m_materialIndices16":
            case "materialIndices16":
            {
                value = instance.m_materialIndices16;
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
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<float> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_indices16":
            case "indices16":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_indices16 = castValue;
                return true;
            }
            case "m_indices32":
            case "indices32":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_indices32 = castValue;
                return true;
            }
            case "m_materialIndices":
            case "materialIndices":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_materialIndices = castValue;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_materialIndices16":
            case "materialIndices16":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_materialIndices16 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
