// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclObjectSpaceDeformerSevenBlendEntryBlockData : HavokData<hclObjectSpaceDeformer.SevenBlendEntryBlock> 
{
    private static readonly System.Reflection.FieldInfo _vertexIndicesInfo = typeof(hclObjectSpaceDeformer.SevenBlendEntryBlock).GetField("m_vertexIndices")!;
    private static readonly System.Reflection.FieldInfo _boneIndicesInfo = typeof(hclObjectSpaceDeformer.SevenBlendEntryBlock).GetField("m_boneIndices")!;
    private static readonly System.Reflection.FieldInfo _boneWeightsInfo = typeof(hclObjectSpaceDeformer.SevenBlendEntryBlock).GetField("m_boneWeights")!;
    public hclObjectSpaceDeformerSevenBlendEntryBlockData(HavokType type, hclObjectSpaceDeformer.SevenBlendEntryBlock instance) : base(type, instance) {}

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
            case "m_boneIndices":
            case "boneIndices":
            {
                if (instance.m_boneIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (instance.m_boneWeights is not TGet castValue) return false;
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
                if (value is not ushort[] castValue || castValue.Length != 16) return false;
                try
                {
                    _vertexIndicesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_boneIndices":
            case "boneIndices":
            {
                if (value is not ushort[] castValue || castValue.Length != 112) return false;
                try
                {
                    _boneIndicesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (value is not ushort[] castValue || castValue.Length != 112) return false;
                try
                {
                    _boneWeightsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
