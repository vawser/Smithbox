// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkDetailVersioningDependentTypeListData : HavokData<HKLib.hk2019.hk.Detail.VersioningDependentTypeList> 
{
    public hkDetailVersioningDependentTypeListData(HavokType type, HKLib.hk2019.hk.Detail.VersioningDependentTypeList instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (instance.m_value is not TGet castValue) return false;
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
            case "m_value":
            case "value":
            {
                if (value is not List<HKLib.hk2019.hk.Detail.VersioningDependentType> castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
