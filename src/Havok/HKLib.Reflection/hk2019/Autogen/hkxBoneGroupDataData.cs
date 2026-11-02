// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxBoneGroupDataData : HavokData<HKLib.hk2019.hkxBoneGroupData> 
{
    public hkxBoneGroupDataData(HavokType type, HKLib.hk2019.hkxBoneGroupData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_groupNames":
            case "groupNames":
            {
                if (instance.m_groupNames is not TGet castValue) return false;
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
            case "m_groupNames":
            case "groupNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_groupNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
