// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclGatherSomeVerticesOperatorVertexPairData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclGatherSomeVerticesOperator.VertexPair>
{
    public hclGatherSomeVerticesOperatorVertexPairData(Havok.hk2014.hclGatherSomeVerticesOperator.VertexPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_indexInput":
            case "indexInput":
            {
                value = instance.m_indexInput;
                return true;
            }
            case "m_indexOutput":
            case "indexOutput":
            {
                value = instance.m_indexOutput;
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
