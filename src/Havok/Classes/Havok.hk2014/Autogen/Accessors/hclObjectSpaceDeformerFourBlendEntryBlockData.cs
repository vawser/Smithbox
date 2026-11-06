// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclObjectSpaceDeformerFourBlendEntryBlockData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclObjectSpaceDeformer.FourBlendEntryBlock>
{
    private static readonly System.Reflection.FieldInfo _vertexIndicesInfo = typeof(Havok.hk2014.hclObjectSpaceDeformer.FourBlendEntryBlock).GetField("m_vertexIndices")!;
    private static readonly System.Reflection.FieldInfo _boneIndicesInfo = typeof(Havok.hk2014.hclObjectSpaceDeformer.FourBlendEntryBlock).GetField("m_boneIndices")!;
    private static readonly System.Reflection.FieldInfo _boneWeightsInfo = typeof(Havok.hk2014.hclObjectSpaceDeformer.FourBlendEntryBlock).GetField("m_boneWeights")!;
    public hclObjectSpaceDeformerFourBlendEntryBlockData(Havok.hk2014.hclObjectSpaceDeformer.FourBlendEntryBlock instance) : base(instance) { }

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
                if (value is not ushort[] castValue || castValue.Length != 64) return false;
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
                if (value is not byte[] castValue || castValue.Length != 64) return false;
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
