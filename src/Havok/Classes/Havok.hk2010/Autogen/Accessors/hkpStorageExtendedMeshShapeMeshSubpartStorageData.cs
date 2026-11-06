// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpStorageExtendedMeshShapeMeshSubpartStorageData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpStorageExtendedMeshShapeMeshSubpartStorage>
{
    public hkpStorageExtendedMeshShapeMeshSubpartStorageData(Havok.hk2010.hkpStorageExtendedMeshShapeMeshSubpartStorage instance) : base(instance) { }

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
            case "m_indices8":
            case "indices8":
            {
                value = instance.m_indices8;
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
            case "m_namedMaterials":
            case "namedMaterials":
            {
                value = instance.m_namedMaterials;
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
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_indices8":
            case "indices8":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_indices8 = castValue;
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
                if (value is not List<Havok.hk2010.hkpStorageExtendedMeshShapeMaterial> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_namedMaterials":
            case "namedMaterials":
            {
                if (value is not List<Havok.hk2010.hkpNamedMeshMaterial> castValue) return false;
                instance.m_namedMaterials = castValue;
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
