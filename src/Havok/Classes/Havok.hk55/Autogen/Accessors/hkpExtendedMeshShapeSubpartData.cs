// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpExtendedMeshShapeSubpartData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpExtendedMeshShapeSubpart>
{
    public hkpExtendedMeshShapeSubpartData(Havok.hk55.hkpExtendedMeshShapeSubpart instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_materialIndexStridingType":
            case "materialIndexStridingType":
            {
                value = instance.m_materialIndexStridingType;
                return true;
            }
            case "m_materialIndexStriding":
            case "materialIndexStriding":
            {
                value = instance.m_materialIndexStriding;
                return true;
            }
            case "m_numMaterials":
            case "numMaterials":
            {
                value = instance.m_numMaterials;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk55.hkpExtendedMeshShape.SubpartType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk55.hkpExtendedMeshShape.SubpartType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_materialIndexStridingType":
            case "materialIndexStridingType":
            {
                if (value is Havok.hk55.hkpExtendedMeshShape.MaterialIndexStridingType castValue)
                {
                    instance.m_materialIndexStridingType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_materialIndexStridingType = (Havok.hk55.hkpExtendedMeshShape.MaterialIndexStridingType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_materialIndexStriding":
            case "materialIndexStriding":
            {
                if (value is not ushort castValue) return false;
                instance.m_materialIndexStriding = castValue;
                return true;
            }
            case "m_numMaterials":
            case "numMaterials":
            {
                if (value is not ushort castValue) return false;
                instance.m_numMaterials = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
