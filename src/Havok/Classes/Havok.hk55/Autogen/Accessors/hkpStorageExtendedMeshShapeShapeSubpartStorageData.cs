// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpStorageExtendedMeshShapeShapeSubpartStorageData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpStorageExtendedMeshShapeShapeSubpartStorage>
{
    public hkpStorageExtendedMeshShapeShapeSubpartStorageData(Havok.hk55.hkpStorageExtendedMeshShapeShapeSubpartStorage instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
                if (value is not List<Havok.hk55.hkpStorageExtendedMeshShapeMaterial> castValue) return false;
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
