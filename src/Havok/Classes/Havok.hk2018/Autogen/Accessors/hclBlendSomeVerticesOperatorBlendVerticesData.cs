// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBlendSomeVerticesOperatorBlendVerticesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBlendSomeVerticesOperator.BlendVertices>
{
    public hclBlendSomeVerticesOperatorBlendVerticesData(Havok.hk2018.hclBlendSomeVerticesOperator.BlendVertices instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndices":
            case "vertexIndices":
            {
                value = instance.m_vertexIndices;
                return true;
            }
            case "m_constBlendWeight":
            case "constBlendWeight":
            {
                value = instance.m_constBlendWeight;
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
