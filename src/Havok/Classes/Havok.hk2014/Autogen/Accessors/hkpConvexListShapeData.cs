// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpConvexListShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpConvexListShape>
{
    public hkpConvexListShapeData(Havok.hk2014.hkpConvexListShape instance) : base(instance) { }

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
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_minDistanceToUseConvexHullForGetClosestPoints":
            case "minDistanceToUseConvexHullForGetClosestPoints":
            {
                value = instance.m_minDistanceToUseConvexHullForGetClosestPoints;
                return true;
            }
            case "m_aabbHalfExtents":
            case "aabbHalfExtents":
            {
                value = instance.m_aabbHalfExtents;
                return true;
            }
            case "m_aabbCenter":
            case "aabbCenter":
            {
                value = instance.m_aabbCenter;
                return true;
            }
            case "m_useCachedAabb":
            case "useCachedAabb":
            {
                value = instance.m_useCachedAabb;
                return true;
            }
            case "m_childShapes":
            case "childShapes":
            {
                value = instance.m_childShapes;
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
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_minDistanceToUseConvexHullForGetClosestPoints":
            case "minDistanceToUseConvexHullForGetClosestPoints":
            {
                if (value is not float castValue) return false;
                instance.m_minDistanceToUseConvexHullForGetClosestPoints = castValue;
                return true;
            }
            case "m_aabbHalfExtents":
            case "aabbHalfExtents":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_aabbHalfExtents = castValue;
                return true;
            }
            case "m_aabbCenter":
            case "aabbCenter":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_aabbCenter = castValue;
                return true;
            }
            case "m_useCachedAabb":
            case "useCachedAabb":
            {
                if (value is not bool castValue) return false;
                instance.m_useCachedAabb = castValue;
                return true;
            }
            case "m_childShapes":
            case "childShapes":
            {
                if (value is not List<Havok.hk2014.hkpConvexShape?> castValue) return false;
                instance.m_childShapes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
