// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpExtendedMeshShapeTrianglesSubpartData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpExtendedMeshShapeTrianglesSubpart>
{
    public hkpExtendedMeshShapeTrianglesSubpartData(Havok.hk55.hkpExtendedMeshShapeTrianglesSubpart instance) : base(instance) { }

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
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_numTriangleShapes":
            case "numTriangleShapes":
            {
                value = instance.m_numTriangleShapes;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
                return true;
            }
            case "m_vertexStriding":
            case "vertexStriding":
            {
                value = instance.m_vertexStriding;
                return true;
            }
            case "m_triangleOffset":
            case "triangleOffset":
            {
                value = instance.m_triangleOffset;
                return true;
            }
            case "m_indexStriding":
            case "indexStriding":
            {
                value = instance.m_indexStriding;
                return true;
            }
            case "m_stridingType":
            case "stridingType":
            {
                value = instance.m_stridingType;
                return true;
            }
            case "m_flipAlternateTriangles":
            case "flipAlternateTriangles":
            {
                value = instance.m_flipAlternateTriangles;
                return true;
            }
            case "m_extrusion":
            case "extrusion":
            {
                value = instance.m_extrusion;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_numTriangleShapes":
            case "numTriangleShapes":
            {
                if (value is not int castValue) return false;
                instance.m_numTriangleShapes = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not int castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            case "m_vertexStriding":
            case "vertexStriding":
            {
                if (value is not ushort castValue) return false;
                instance.m_vertexStriding = castValue;
                return true;
            }
            case "m_triangleOffset":
            case "triangleOffset":
            {
                if (value is not int castValue) return false;
                instance.m_triangleOffset = castValue;
                return true;
            }
            case "m_indexStriding":
            case "indexStriding":
            {
                if (value is not ushort castValue) return false;
                instance.m_indexStriding = castValue;
                return true;
            }
            case "m_stridingType":
            case "stridingType":
            {
                if (value is Havok.hk55.hkpExtendedMeshShape.IndexStridingType castValue)
                {
                    instance.m_stridingType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_stridingType = (Havok.hk55.hkpExtendedMeshShape.IndexStridingType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_flipAlternateTriangles":
            case "flipAlternateTriangles":
            {
                if (value is not sbyte castValue) return false;
                instance.m_flipAlternateTriangles = castValue;
                return true;
            }
            case "m_extrusion":
            case "extrusion":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_extrusion = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk55.hkQsTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
