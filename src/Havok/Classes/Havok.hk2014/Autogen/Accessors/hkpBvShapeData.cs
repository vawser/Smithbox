// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBvShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBvShape>
{
    public hkpBvShapeData(Havok.hk2014.hkpBvShape instance) : base(instance) { }

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
            case "m_boundingVolumeShape":
            case "boundingVolumeShape":
            {
                value = instance.m_boundingVolumeShape;
                return true;
            }
            case "m_childShape":
            case "childShape":
            {
                value = instance.m_childShape;
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
            case "m_boundingVolumeShape":
            case "boundingVolumeShape":
            {
                if (value is null)
                {
                    instance.m_boundingVolumeShape = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpShape castValue)
                {
                    instance.m_boundingVolumeShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_childShape":
            case "childShape":
            {
                if (value is not Havok.hk2014.hkpSingleShapeContainer castValue) return false;
                instance.m_childShape = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
