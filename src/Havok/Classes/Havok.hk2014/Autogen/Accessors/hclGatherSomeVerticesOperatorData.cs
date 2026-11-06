// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclGatherSomeVerticesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclGatherSomeVerticesOperator>
{
    public hclGatherSomeVerticesOperatorData(Havok.hk2014.hclGatherSomeVerticesOperator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_vertexPairs":
            case "vertexPairs":
            {
                value = instance.m_vertexPairs;
                return true;
            }
            case "m_inputBufferIdx":
            case "inputBufferIdx":
            {
                value = instance.m_inputBufferIdx;
                return true;
            }
            case "m_outputBufferIdx":
            case "outputBufferIdx":
            {
                value = instance.m_outputBufferIdx;
                return true;
            }
            case "m_gatherNormals":
            case "gatherNormals":
            {
                value = instance.m_gatherNormals;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexPairs":
            case "vertexPairs":
            {
                if (value is not List<Havok.hk2014.hclGatherSomeVerticesOperator.VertexPair> castValue) return false;
                instance.m_vertexPairs = castValue;
                return true;
            }
            case "m_inputBufferIdx":
            case "inputBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_inputBufferIdx = castValue;
                return true;
            }
            case "m_outputBufferIdx":
            case "outputBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_outputBufferIdx = castValue;
                return true;
            }
            case "m_gatherNormals":
            case "gatherNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_gatherNormals = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
