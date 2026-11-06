// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpListShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpListShape>
{
    private static readonly System.Reflection.FieldInfo _enabledChildrenInfo = typeof(Havok.hk2014.hkpListShape).GetField("m_enabledChildren")!;
    public hkpListShapeData(Havok.hk2014.hkpListShape instance) : base(instance) { }

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
            case "m_childInfo":
            case "childInfo":
            {
                value = instance.m_childInfo;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_numDisabledChildren":
            case "numDisabledChildren":
            {
                value = instance.m_numDisabledChildren;
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
            case "m_enabledChildren":
            case "enabledChildren":
            {
                value = instance.m_enabledChildren;
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
            case "m_childInfo":
            case "childInfo":
            {
                if (value is not List<Havok.hk2014.hkpListShapeChildInfo> castValue) return false;
                instance.m_childInfo = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not ushort castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_numDisabledChildren":
            case "numDisabledChildren":
            {
                if (value is not ushort castValue) return false;
                instance.m_numDisabledChildren = castValue;
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
            case "m_enabledChildren":
            case "enabledChildren":
            {
                if (value is not uint[] castValue || castValue.Length != 8) return false;
                try
                {
                    _enabledChildrenInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
