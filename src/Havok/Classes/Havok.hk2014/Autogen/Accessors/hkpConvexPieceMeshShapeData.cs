// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpConvexPieceMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpConvexPieceMeshShape>
{
    public hkpConvexPieceMeshShapeData(Havok.hk2014.hkpConvexPieceMeshShape instance) : base(instance) { }

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
            case "m_convexPieceStream":
            case "convexPieceStream":
            {
                value = instance.m_convexPieceStream;
                return true;
            }
            case "m_displayMesh":
            case "displayMesh":
            {
                value = instance.m_displayMesh;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
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
            case "m_convexPieceStream":
            case "convexPieceStream":
            {
                if (value is null)
                {
                    instance.m_convexPieceStream = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpConvexPieceStreamData castValue)
                {
                    instance.m_convexPieceStream = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayMesh":
            case "displayMesh":
            {
                if (value is null)
                {
                    instance.m_displayMesh = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpShapeCollection castValue)
                {
                    instance.m_displayMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
