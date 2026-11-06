// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStorageSetupMeshSectionBoneInfluencesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStorageSetupMeshSection.BoneInfluences>
{
    public hclStorageSetupMeshSectionBoneInfluencesData(Havok.hk2018.hclStorageSetupMeshSection.BoneInfluences instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
