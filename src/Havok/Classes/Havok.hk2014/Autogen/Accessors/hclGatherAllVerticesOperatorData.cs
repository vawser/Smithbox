// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclGatherAllVerticesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclGatherAllVerticesOperator>
{
    public hclGatherAllVerticesOperatorData(Havok.hk2014.hclGatherAllVerticesOperator instance) : base(instance) { }

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
            case "m_vertexInputFromVertexOutput":
            case "vertexInputFromVertexOutput":
            {
                value = instance.m_vertexInputFromVertexOutput;
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
            case "m_partialGather":
            case "partialGather":
            {
                value = instance.m_partialGather;
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
            case "m_vertexInputFromVertexOutput":
            case "vertexInputFromVertexOutput":
            {
                if (value is not List<short> castValue) return false;
                instance.m_vertexInputFromVertexOutput = castValue;
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
            case "m_partialGather":
            case "partialGather":
            {
                if (value is not bool castValue) return false;
                instance.m_partialGather = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
