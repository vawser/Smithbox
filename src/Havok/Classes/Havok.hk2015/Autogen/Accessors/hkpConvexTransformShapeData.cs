// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpConvexTransformShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpConvexTransformShape>
{
    public hkpConvexTransformShapeData(Havok.hk2015.hkpConvexTransformShape instance) : base(instance) { }

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
            case "m_childShape":
            case "childShape":
            {
                value = instance.m_childShape;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_extraScale":
            case "extraScale":
            {
                value = instance.m_extraScale;
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
                if (value is not byte castValue) return false;
                instance.m_dispatchType = castValue;
                return true;
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
                if (value is not byte castValue) return false;
                instance.m_shapeInfoCodecType = castValue;
                return true;
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
            case "m_childShape":
            case "childShape":
            {
                if (value is not Havok.hk2015.hkpSingleShapeContainer castValue) return false;
                instance.m_childShape = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2015.hkQsTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_extraScale":
            case "extraScale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_extraScale = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
