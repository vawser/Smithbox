// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSimpleMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSimpleMeshShape>
{
    public hkpSimpleMeshShapeData(Havok.hk2014.hkpSimpleMeshShape instance) : base(instance) { }

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
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                value = instance.m_triangles;
                return true;
            }
            case "m_materialIndices":
            case "materialIndices":
            {
                value = instance.m_materialIndices;
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
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<Havok.hk2014.hkpSimpleMeshShapeTriangle> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            case "m_materialIndices":
            case "materialIndices":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_materialIndices = castValue;
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
            default:
            return false;
        }
    }

}
