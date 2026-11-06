// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBvCompressedMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBvCompressedMeshShape>
{
    public hkpBvCompressedMeshShapeData(Havok.hk2014.hkpBvCompressedMeshShape instance) : base(instance) { }

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
            case "m_bvTreeType":
            case "bvTreeType":
            {
                value = instance.m_bvTreeType;
                return true;
            }
            case "m_convexRadius":
            case "convexRadius":
            {
                value = instance.m_convexRadius;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                value = instance.m_weldingType;
                return true;
            }
            case "m_hasPerPrimitiveCollisionFilterInfo":
            case "hasPerPrimitiveCollisionFilterInfo":
            {
                value = instance.m_hasPerPrimitiveCollisionFilterInfo;
                return true;
            }
            case "m_hasPerPrimitiveUserData":
            case "hasPerPrimitiveUserData":
            {
                value = instance.m_hasPerPrimitiveUserData;
                return true;
            }
            case "m_collisionFilterInfoPalette":
            case "collisionFilterInfoPalette":
            {
                value = instance.m_collisionFilterInfoPalette;
                return true;
            }
            case "m_userDataPalette":
            case "userDataPalette":
            {
                value = instance.m_userDataPalette;
                return true;
            }
            case "m_userStringPalette":
            case "userStringPalette":
            {
                value = instance.m_userStringPalette;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                value = instance.m_tree;
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
            case "m_bvTreeType":
            case "bvTreeType":
            {
                if (value is Havok.hk2014.hkpBvTreeShape.BvTreeType castValue)
                {
                    instance.m_bvTreeType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_bvTreeType = (Havok.hk2014.hkpBvTreeShape.BvTreeType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_convexRadius":
            case "convexRadius":
            {
                if (value is not float castValue) return false;
                instance.m_convexRadius = castValue;
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
            case "m_hasPerPrimitiveCollisionFilterInfo":
            case "hasPerPrimitiveCollisionFilterInfo":
            {
                if (value is not bool castValue) return false;
                instance.m_hasPerPrimitiveCollisionFilterInfo = castValue;
                return true;
            }
            case "m_hasPerPrimitiveUserData":
            case "hasPerPrimitiveUserData":
            {
                if (value is not bool castValue) return false;
                instance.m_hasPerPrimitiveUserData = castValue;
                return true;
            }
            case "m_collisionFilterInfoPalette":
            case "collisionFilterInfoPalette":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_collisionFilterInfoPalette = castValue;
                return true;
            }
            case "m_userDataPalette":
            case "userDataPalette":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_userDataPalette = castValue;
                return true;
            }
            case "m_userStringPalette":
            case "userStringPalette":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_userStringPalette = castValue;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                if (value is not Havok.hk2014.hkpBvCompressedMeshShapeTree castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
