// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclGatherSomeVerticesOperatorVertexPairData : HavokData<hclGatherSomeVerticesOperator.VertexPair> 
{
    public hclGatherSomeVerticesOperatorVertexPairData(HavokType type, hclGatherSomeVerticesOperator.VertexPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_indexInput":
            case "indexInput":
            {
                if (instance.m_indexInput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexOutput":
            case "indexOutput":
            {
                if (instance.m_indexOutput is not TGet castValue) return false;
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
            case "m_indexInput":
            case "indexInput":
            {
                if (value is not ushort castValue) return false;
                instance.m_indexInput = castValue;
                return true;
            }
            case "m_indexOutput":
            case "indexOutput":
            {
                if (value is not ushort castValue) return false;
                instance.m_indexOutput = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
