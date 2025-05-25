// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSkinOperatorBoneInfluenceData : HavokData<hclSkinOperator.BoneInfluence> 
{
    public hclSkinOperatorBoneInfluenceData(HavokType type, hclSkinOperator.BoneInfluence instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneIndex":
            case "boneIndex":
            {
                if (instance.m_boneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
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
