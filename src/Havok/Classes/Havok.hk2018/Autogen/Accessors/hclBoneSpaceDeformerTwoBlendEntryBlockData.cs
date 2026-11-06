// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBoneSpaceDeformerTwoBlendEntryBlockData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBoneSpaceDeformer.TwoBlendEntryBlock>
{
    private static readonly System.Reflection.FieldInfo _vertexIndicesInfo = typeof(Havok.hk2018.hclBoneSpaceDeformer.TwoBlendEntryBlock).GetField("m_vertexIndices")!;
    private static readonly System.Reflection.FieldInfo _boneIndicesInfo = typeof(Havok.hk2018.hclBoneSpaceDeformer.TwoBlendEntryBlock).GetField("m_boneIndices")!;
    public hclBoneSpaceDeformerTwoBlendEntryBlockData(Havok.hk2018.hclBoneSpaceDeformer.TwoBlendEntryBlock instance) : base(instance) { }

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
                if (value is not ushort[] castValue || castValue.Length != 8) return false;
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
                if (value is not ushort[] castValue || castValue.Length != 16) return false;
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
            default:
            return false;
        }
    }

}
