// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpStaticCompoundShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpStaticCompoundShape>
{
    public hkpStaticCompoundShapeData(Havok.hk2014.hkpStaticCompoundShape instance) : base(instance) { }

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
            case "m_numBitsForChildShapeKey":
            case "numBitsForChildShapeKey":
            {
                value = instance.m_numBitsForChildShapeKey;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                value = instance.m_instances;
                return true;
            }
            case "m_instanceExtraInfos":
            case "instanceExtraInfos":
            {
                value = instance.m_instanceExtraInfos;
                return true;
            }
            case "m_disabledLargeShapeKeyTable":
            case "disabledLargeShapeKeyTable":
            {
                value = instance.m_disabledLargeShapeKeyTable;
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
            case "m_numBitsForChildShapeKey":
            case "numBitsForChildShapeKey":
            {
                if (value is not sbyte castValue) return false;
                instance.m_numBitsForChildShapeKey = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not List<Havok.hk2014.hkpStaticCompoundShapeInstance> castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            case "m_instanceExtraInfos":
            case "instanceExtraInfos":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_instanceExtraInfos = castValue;
                return true;
            }
            case "m_disabledLargeShapeKeyTable":
            case "disabledLargeShapeKeyTable":
            {
                if (value is not Havok.hk2014.hkpShapeKeyTable castValue) return false;
                instance.m_disabledLargeShapeKeyTable = castValue;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                if (value is not Havok.hk2014.hkcdStaticTreeDefaultTreeStorage6 castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
