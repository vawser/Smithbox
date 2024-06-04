// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBlendSomeVerticesOperatorBlendEntryData : HavokData<hclBlendSomeVerticesOperator.BlendEntry> 
{
    public hclBlendSomeVerticesOperatorBlendEntryData(HavokType type, hclBlendSomeVerticesOperator.BlendEntry instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndex":
            case "vertexIndex":
            {
                if (instance.m_vertexIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendWeight":
            case "blendWeight":
            {
                if (instance.m_blendWeight is not TGet castValue) return false;
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
