// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpExtendedMeshShapeShapesSubpartData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpExtendedMeshShapeShapesSubpart>
{
    public hkpExtendedMeshShapeShapesSubpartData(Havok.hk55.hkpExtendedMeshShapeShapesSubpart instance) : base(instance) { }

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
            case "m_childShapes":
            case "childShapes":
            {
                value = instance.m_childShapes;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                value = instance.m_rotation;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                value = instance.m_translation;
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
            case "m_childShapes":
            case "childShapes":
            {
                if (value is not List<Havok.hk55.hkpConvexShape?> castValue) return false;
                instance.m_childShapes = castValue;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
