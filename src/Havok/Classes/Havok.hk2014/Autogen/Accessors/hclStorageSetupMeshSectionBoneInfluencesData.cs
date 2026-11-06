// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStorageSetupMeshSectionBoneInfluencesData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStorageSetupMeshSection.BoneInfluences>
{
    public hclStorageSetupMeshSectionBoneInfluencesData(Havok.hk2014.hclStorageSetupMeshSection.BoneInfluences instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneIndices":
            case "boneIndices":
            {
                value = instance.m_boneIndices;
                return true;
            }
            case "m_weights":
            case "weights":
            {
                value = instance.m_weights;
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
            case "m_boneIndices":
            case "boneIndices":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_boneIndices = castValue;
                return true;
            }
            case "m_weights":
            case "weights":
            {
                if (value is not List<float> castValue) return false;
                instance.m_weights = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
