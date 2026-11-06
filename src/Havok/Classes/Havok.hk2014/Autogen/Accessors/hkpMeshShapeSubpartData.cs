// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMeshShapeSubpartData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMeshShapeSubpart>
{
    public hkpMeshShapeSubpartData(Havok.hk2014.hkpMeshShapeSubpart instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexStriding":
            case "vertexStriding":
            {
                value = instance.m_vertexStriding;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
                return true;
            }
            case "m_stridingType":
            case "stridingType":
            {
                value = instance.m_stridingType;
                return true;
            }
            case "m_materialIndexStridingType":
            case "materialIndexStridingType":
            {
                value = instance.m_materialIndexStridingType;
                return true;
            }
            case "m_indexStriding":
            case "indexStriding":
            {
                value = instance.m_indexStriding;
                return true;
            }
            case "m_flipAlternateTriangles":
            case "flipAlternateTriangles":
            {
                value = instance.m_flipAlternateTriangles;
                return true;
            }
            case "m_numTriangles":
            case "numTriangles":
            {
                value = instance.m_numTriangles;
                return true;
            }
            case "m_materialIndexStriding":
            case "materialIndexStriding":
            {
                value = instance.m_materialIndexStriding;
                return true;
            }
            case "m_materialStriding":
            case "materialStriding":
            {
                value = instance.m_materialStriding;
                return true;
            }
            case "m_numMaterials":
            case "numMaterials":
            {
                value = instance.m_numMaterials;
                return true;
            }
            case "m_triangleOffset":
            case "triangleOffset":
            {
                value = instance.m_triangleOffset;
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
            case "m_vertexStriding":
            case "vertexStriding":
            {
                if (value is not int castValue) return false;
                instance.m_vertexStriding = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not int castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            case "m_stridingType":
            case "stridingType":
            {
                if (value is Havok.hk2014.hkpMeshShape.MeshShapeIndexStridingType castValue)
                {
                    instance.m_stridingType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_stridingType = (Havok.hk2014.hkpMeshShape.MeshShapeIndexStridingType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_materialIndexStridingType":
            case "materialIndexStridingType":
            {
                if (value is Havok.hk2014.hkpMeshShape.MeshShapeMaterialIndexStridingType castValue)
                {
                    instance.m_materialIndexStridingType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_materialIndexStridingType = (Havok.hk2014.hkpMeshShape.MeshShapeMaterialIndexStridingType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_indexStriding":
            case "indexStriding":
            {
                if (value is not int castValue) return false;
                instance.m_indexStriding = castValue;
                return true;
            }
            case "m_flipAlternateTriangles":
            case "flipAlternateTriangles":
            {
                if (value is not int castValue) return false;
                instance.m_flipAlternateTriangles = castValue;
                return true;
            }
            case "m_numTriangles":
            case "numTriangles":
            {
                if (value is not int castValue) return false;
                instance.m_numTriangles = castValue;
                return true;
            }
            case "m_materialIndexStriding":
            case "materialIndexStriding":
            {
                if (value is not int castValue) return false;
                instance.m_materialIndexStriding = castValue;
                return true;
            }
            case "m_materialStriding":
            case "materialStriding":
            {
                if (value is not int castValue) return false;
                instance.m_materialStriding = castValue;
                return true;
            }
            case "m_numMaterials":
            case "numMaterials":
            {
                if (value is not int castValue) return false;
                instance.m_numMaterials = castValue;
                return true;
            }
            case "m_triangleOffset":
            case "triangleOffset":
            {
                if (value is not int castValue) return false;
                instance.m_triangleOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
