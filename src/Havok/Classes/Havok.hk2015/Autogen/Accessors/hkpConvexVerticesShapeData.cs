// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpConvexVerticesShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpConvexVerticesShape>
{
    public hkpConvexVerticesShapeData(Havok.hk2015.hkpConvexVerticesShape instance) : base(instance) { }

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
            case "m_rotatedVertices":
            case "rotatedVertices":
            {
                value = instance.m_rotatedVertices;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
                return true;
            }
            case "m_planeEquations":
            case "planeEquations":
            {
                value = instance.m_planeEquations;
                return true;
            }
            case "m_connectivity":
            case "connectivity":
            {
                value = instance.m_connectivity;
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
            case "m_rotatedVertices":
            case "rotatedVertices":
            {
                if (value is not List<Matrix3x3> castValue) return false;
                instance.m_rotatedVertices = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not int castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            case "m_planeEquations":
            case "planeEquations":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_planeEquations = castValue;
                return true;
            }
            case "m_connectivity":
            case "connectivity":
            {
                if (value is null)
                {
                    instance.m_connectivity = default;
                    return true;
                }
                if (value is Havok.hk2015.hkpConvexVerticesConnectivity castValue)
                {
                    instance.m_connectivity = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
