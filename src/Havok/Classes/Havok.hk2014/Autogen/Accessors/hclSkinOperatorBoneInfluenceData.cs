// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSkinOperatorBoneInfluenceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSkinOperator.BoneInfluence>
{
    public hclSkinOperatorBoneInfluenceData(Havok.hk2014.hclSkinOperator.BoneInfluence instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneIndex":
            case "boneIndex":
            {
                value = instance.m_boneIndex;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
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
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not byte castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
