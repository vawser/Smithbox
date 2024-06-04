// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBlendSomeVerticesOperatorBlendVerticesData : HavokData<hclBlendSomeVerticesOperator.BlendVertices> 
{
    public hclBlendSomeVerticesOperatorBlendVerticesData(HavokType type, hclBlendSomeVerticesOperator.BlendVertices instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndices":
            case "vertexIndices":
            {
                if (instance.m_vertexIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constBlendWeight":
            case "constBlendWeight":
            {
                if (instance.m_constBlendWeight is not TGet castValue) return false;
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
            case "m_vertexIndices":
            case "vertexIndices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_vertexIndices = castValue;
                return true;
            }
            case "m_constBlendWeight":
            case "constBlendWeight":
            {
                if (value is not float castValue) return false;
                instance.m_constBlendWeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
