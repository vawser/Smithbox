// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclObjectSpaceDeformerFiveBlendEntryBlockData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclObjectSpaceDeformer.FiveBlendEntryBlock>
{
    private static readonly System.Reflection.FieldInfo _vertexIndicesInfo = typeof(Havok.hk2016.hclObjectSpaceDeformer.FiveBlendEntryBlock).GetField("m_vertexIndices")!;
    private static readonly System.Reflection.FieldInfo _boneIndicesInfo = typeof(Havok.hk2016.hclObjectSpaceDeformer.FiveBlendEntryBlock).GetField("m_boneIndices")!;
    private static readonly System.Reflection.FieldInfo _boneWeightsInfo = typeof(Havok.hk2016.hclObjectSpaceDeformer.FiveBlendEntryBlock).GetField("m_boneWeights")!;
    public hclObjectSpaceDeformerFiveBlendEntryBlockData(Havok.hk2016.hclObjectSpaceDeformer.FiveBlendEntryBlock instance) : base(instance) { }

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
            case "m_boneIndices":
            case "boneIndices":
            {
                value = instance.m_boneIndices;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                value = instance.m_boneWeights;
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
                if (value is not ushort[] castValue || castValue.Length != 80) return false;
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
                if (value is not ushort[] castValue || castValue.Length != 80) return false;
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
