// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpTriangleShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpTriangleShape>
{
    public hkpTriangleShapeData(Havok.hk2014.hkpTriangleShape instance) : base(instance) { }

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
            case "m_weldingInfo":
            case "weldingInfo":
            {
                value = instance.m_weldingInfo;
                return true;
            }
            case "m_weldingType":
            case "weldingType":
            {
                value = instance.m_weldingType;
                return true;
            }
            case "m_isExtruded":
            case "isExtruded":
            {
                value = instance.m_isExtruded;
                return true;
            }
            case "m_vertexA":
            case "vertexA":
            {
                value = instance.m_vertexA;
                return true;
            }
            case "m_vertexB":
            case "vertexB":
            {
                value = instance.m_vertexB;
                return true;
            }
            case "m_vertexC":
            case "vertexC":
            {
                value = instance.m_vertexC;
                return true;
            }
            case "m_extrusion":
            case "extrusion":
            {
                value = instance.m_extrusion;
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
            case "m_weldingInfo":
            case "weldingInfo":
            {
                if (value is not ushort castValue) return false;
                instance.m_weldingInfo = castValue;
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
            case "m_isExtruded":
            case "isExtruded":
            {
                if (value is not byte castValue) return false;
                instance.m_isExtruded = castValue;
                return true;
            }
            case "m_vertexA":
            case "vertexA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vertexA = castValue;
                return true;
            }
            case "m_vertexB":
            case "vertexB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vertexB = castValue;
                return true;
            }
            case "m_vertexC":
            case "vertexC":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vertexC = castValue;
                return true;
            }
            case "m_extrusion":
            case "extrusion":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_extrusion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
