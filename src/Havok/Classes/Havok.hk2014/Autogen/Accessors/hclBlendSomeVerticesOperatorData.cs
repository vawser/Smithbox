// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBlendSomeVerticesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBlendSomeVerticesOperator>
{
    public hclBlendSomeVerticesOperatorData(Havok.hk2014.hclBlendSomeVerticesOperator instance) : base(instance) { }

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
            case "m_blendEntries":
            case "blendEntries":
            {
                value = instance.m_blendEntries;
                return true;
            }
            case "m_bufferIdx_A":
            case "bufferIdx_A":
            {
                value = instance.m_bufferIdx_A;
                return true;
            }
            case "m_bufferIdx_B":
            case "bufferIdx_B":
            {
                value = instance.m_bufferIdx_B;
                return true;
            }
            case "m_bufferIdx_C":
            case "bufferIdx_C":
            {
                value = instance.m_bufferIdx_C;
                return true;
            }
            case "m_blendNormals":
            case "blendNormals":
            {
                value = instance.m_blendNormals;
                return true;
            }
            case "m_blendTangents":
            case "blendTangents":
            {
                value = instance.m_blendTangents;
                return true;
            }
            case "m_blendBitangents":
            case "blendBitangents":
            {
                value = instance.m_blendBitangents;
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
            case "m_blendEntries":
            case "blendEntries":
            {
                if (value is not List<Havok.hk2014.hclBlendSomeVerticesOperator.BlendEntry> castValue) return false;
                instance.m_blendEntries = castValue;
                return true;
            }
            case "m_bufferIdx_A":
            case "bufferIdx_A":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx_A = castValue;
                return true;
            }
            case "m_bufferIdx_B":
            case "bufferIdx_B":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx_B = castValue;
                return true;
            }
            case "m_bufferIdx_C":
            case "bufferIdx_C":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx_C = castValue;
                return true;
            }
            case "m_blendNormals":
            case "blendNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_blendNormals = castValue;
                return true;
            }
            case "m_blendTangents":
            case "blendTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_blendTangents = castValue;
                return true;
            }
            case "m_blendBitangents":
            case "blendBitangents":
            {
                if (value is not bool castValue) return false;
                instance.m_blendBitangents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
