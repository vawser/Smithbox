// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclMeshMeshDeformOperatorTriangleVertexPairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclMeshMeshDeformOperator.TriangleVertexPair>
{
    public hclMeshMeshDeformOperatorTriangleVertexPairData(Havok.hk2018.hclMeshMeshDeformOperator.TriangleVertexPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localPosition":
            case "localPosition":
            {
                value = instance.m_localPosition;
                return true;
            }
            case "m_localNormal":
            case "localNormal":
            {
                value = instance.m_localNormal;
                return true;
            }
            case "m_triangleIndex":
            case "triangleIndex":
            {
                value = instance.m_triangleIndex;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
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
            case "m_localPosition":
            case "localPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_localPosition = castValue;
                return true;
            }
            case "m_localNormal":
            case "localNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_localNormal = castValue;
                return true;
            }
            case "m_triangleIndex":
            case "triangleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_triangleIndex = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
