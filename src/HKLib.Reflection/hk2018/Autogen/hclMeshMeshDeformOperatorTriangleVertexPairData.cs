// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclMeshMeshDeformOperatorTriangleVertexPairData : HavokData<hclMeshMeshDeformOperator.TriangleVertexPair> 
{
    public hclMeshMeshDeformOperatorTriangleVertexPairData(HavokType type, hclMeshMeshDeformOperator.TriangleVertexPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localPosition":
            case "localPosition":
            {
                if (instance.m_localPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localNormal":
            case "localNormal":
            {
                if (instance.m_localNormal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleIndex":
            case "triangleIndex":
            {
                if (instance.m_triangleIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
