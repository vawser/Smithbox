// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBoneSpaceDeformerThreeBlendEntryBlockData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBoneSpaceDeformer.ThreeBlendEntryBlock>
{
    private static readonly System.Reflection.FieldInfo _vertexIndicesInfo = typeof(Havok.hk2018.hclBoneSpaceDeformer.ThreeBlendEntryBlock).GetField("m_vertexIndices")!;
    private static readonly System.Reflection.FieldInfo _boneIndicesInfo = typeof(Havok.hk2018.hclBoneSpaceDeformer.ThreeBlendEntryBlock).GetField("m_boneIndices")!;
    public hclBoneSpaceDeformerThreeBlendEntryBlockData(Havok.hk2018.hclBoneSpaceDeformer.ThreeBlendEntryBlock instance) : base(instance) { }

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
                if (value is not ushort[] castValue || castValue.Length != 5) return false;
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
                if (value is not ushort[] castValue || castValue.Length != 15) return false;
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
