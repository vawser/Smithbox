// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBlendSomeVerticesOperatorBlendEntryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBlendSomeVerticesOperator.BlendEntry>
{
    public hclBlendSomeVerticesOperatorBlendEntryData(Havok.hk2014.hclBlendSomeVerticesOperator.BlendEntry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndex":
            case "vertexIndex":
            {
                value = instance.m_vertexIndex;
                return true;
            }
            case "m_blendWeight":
            case "blendWeight":
            {
                value = instance.m_blendWeight;
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
            case "m_vertexIndex":
            case "vertexIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_vertexIndex = castValue;
                return true;
            }
            case "m_blendWeight":
            case "blendWeight":
            {
                if (value is not float castValue) return false;
                instance.m_blendWeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
