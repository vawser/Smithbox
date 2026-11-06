// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpCompressedMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpCompressedMeshShape>
{
    public hkpCompressedMeshShapeData(Havok.hk2014.hkpCompressedMeshShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_dispatchType":
            case "dispatchType":
            {
                value = instance.m_dispatchType;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                value = instance.m_bitsPerKey;
                return true;
            }
            case "m_shapeInfoCodecType":
            case "shapeInfoCodecType":
            {
                value = instance.m_shapeInfoCodecType;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_disableWelding":
            case "disableWelding":
            {
                value = instance.m_disableWelding;
                return true;
            }
            case "m_collectionType":
            case "collectionType":
            {
                value = instance.m_collectionType;
                return true;
            }
            case "m_bitsPerIndex":
            case "bitsPerIndex":
            {
                value = instance.m_bitsPerIndex;
                return true;
            }
            case "m_bitsPerWIndex":
            case "bitsPerWIndex":
            {
                value = instance.m_bitsPerWIndex;
                return true;
            }
            case "m_wIndexMask":
            case "wIndexMask":
            {
                value = instance.m_wIndexMask;
                return true;
            }
            case "m_indexMask":
            case "indexMask":
            {
                value = instance.m_indexMask;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                value = instance.m_weldingType;
                return true;
            }
            case "m_materialType":
            case "materialType":
            {
                value = instance.m_materialType;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                value = instance.m_materials;
                return true;
            }
            case "m_materials16":
            case "materials16":
            {
                value = instance.m_materials16;
                return true;
            }
            case "m_materials8":
            case "materials8":
            {
                value = instance.m_materials8;
                return true;
            }
            case "m_transforms":
            case "transforms":
            {
                value = instance.m_transforms;
                return true;
            }
            case "m_bigVertices":
            case "bigVertices":
            {
                value = instance.m_bigVertices;
                return true;
            }
            case "m_bigTriangles":
            case "bigTriangles":
            {
                value = instance.m_bigTriangles;
                return true;
            }
            case "m_chunks":
            case "chunks":
            {
                value = instance.m_chunks;
                return true;
            }
            case "m_convexPieces":
            case "convexPieces":
            {
                value = instance.m_convexPieces;
                return true;
            }
            case "m_error":
            case "error":
            {
                value = instance.m_error;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                value = instance.m_bounds;
                return true;
            }
            case "m_defaultCollisionFilterInfo":
            case "defaultCollisionFilterInfo":
            {
                value = instance.m_defaultCollisionFilterInfo;
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
            case "m_namedMaterials":
            case "namedMaterials":
            {
                value = instance.m_namedMaterials;
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
            case "m_dispatchType":
            case "dispatchType":
            {
                if (value is Havok.hk2014.hkcdShapeDispatchType.ShapeDispatchTypeEnum castValue)
                {
                    instance.m_dispatchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dispatchType = (Havok.hk2014.hkcdShapeDispatchType.ShapeDispatchTypeEnum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                if (value is not byte castValue) return false;
                instance.m_bitsPerKey = castValue;
                return true;
            }
            case "m_shapeInfoCodecType":
            case "shapeInfoCodecType":
            {
                if (value is Havok.hk2014.hkcdShapeInfoCodecType.ShapeInfoCodecTypeEnum castValue)
                {
                    instance.m_shapeInfoCodecType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_shapeInfoCodecType = (Havok.hk2014.hkcdShapeInfoCodecType.ShapeInfoCodecTypeEnum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_disableWelding":
            case "disableWelding":
            {
                if (value is not bool castValue) return false;
                instance.m_disableWelding = castValue;
                return true;
            }
            case "m_collectionType":
            case "collectionType":
            {
                if (value is Havok.hk2014.hkpShapeCollection.CollectionType castValue)
                {
                    instance.m_collectionType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_collectionType = (Havok.hk2014.hkpShapeCollection.CollectionType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_bitsPerIndex":
            case "bitsPerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_bitsPerIndex = castValue;
                return true;
            }
            case "m_bitsPerWIndex":
            case "bitsPerWIndex":
            {
                if (value is not int castValue) return false;
                instance.m_bitsPerWIndex = castValue;
                return true;
            }
            case "m_wIndexMask":
            case "wIndexMask":
            {
                if (value is not int castValue) return false;
                instance.m_wIndexMask = castValue;
                return true;
            }
            case "m_indexMask":
            case "indexMask":
            {
                if (value is not int castValue) return false;
                instance.m_indexMask = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                if (value is Havok.hk2014.hkpWeldingUtility.WeldingType castValue)
                {
                    instance.m_weldingType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_weldingType = (Havok.hk2014.hkpWeldingUtility.WeldingType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_materialType":
            case "materialType":
            {
                if (value is Havok.hk2014.hkpCompressedMeshShape.MaterialType castValue)
                {
                    instance.m_materialType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_materialType = (Havok.hk2014.hkpCompressedMeshShape.MaterialType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_materials":
            case "materials":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_materials16":
            case "materials16":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_materials16 = castValue;
                return true;
            }
            case "m_materials8":
            case "materials8":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_materials8 = castValue;
                return true;
            }
            case "m_transforms":
            case "transforms":
            {
                if (value is not List<Havok.hk2014.hkQsTransform> castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_bigVertices":
            case "bigVertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_bigVertices = castValue;
                return true;
            }
            case "m_bigTriangles":
            case "bigTriangles":
            {
                if (value is not List<Havok.hk2014.hkpCompressedMeshShapeBigTriangle> castValue) return false;
                instance.m_bigTriangles = castValue;
                return true;
            }
            case "m_chunks":
            case "chunks":
            {
                if (value is not List<Havok.hk2014.hkpCompressedMeshShapeChunk> castValue) return false;
                instance.m_chunks = castValue;
                return true;
            }
            case "m_convexPieces":
            case "convexPieces":
            {
                if (value is not List<Havok.hk2014.hkpCompressedMeshShapeConvexPiece> castValue) return false;
                instance.m_convexPieces = castValue;
                return true;
            }
            case "m_error":
            case "error":
            {
                if (value is not float castValue) return false;
                instance.m_error = castValue;
                return true;
            }
            case "m_bounds":
            case "bounds":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_bounds = castValue;
                return true;
            }
            case "m_defaultCollisionFilterInfo":
            case "defaultCollisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_defaultCollisionFilterInfo = castValue;
                return true;
            }
            case "m_materialStriding":
            case "materialStriding":
            {
                if (value is not ushort castValue) return false;
                instance.m_materialStriding = castValue;
                return true;
            }
            case "m_numMaterials":
            case "numMaterials":
            {
                if (value is not ushort castValue) return false;
                instance.m_numMaterials = castValue;
                return true;
            }
            case "m_namedMaterials":
            case "namedMaterials":
            {
                if (value is not List<Havok.hk2014.hkpNamedMeshMaterial> castValue) return false;
                instance.m_namedMaterials = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
