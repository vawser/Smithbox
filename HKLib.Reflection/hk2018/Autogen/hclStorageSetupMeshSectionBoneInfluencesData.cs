// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStorageSetupMeshSectionBoneInfluencesData : HavokData<hclStorageSetupMeshSection.BoneInfluences> 
{
    public hclStorageSetupMeshSectionBoneInfluencesData(HavokType type, hclStorageSetupMeshSection.BoneInfluences instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
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
            case "m_weights":
            case "weights":
            {
                if (instance.m_weights is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
