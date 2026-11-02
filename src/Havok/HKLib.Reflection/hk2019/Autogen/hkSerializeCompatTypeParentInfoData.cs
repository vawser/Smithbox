// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkSerializeCompatTypeParentInfoData : HavokData<HKLib.hk2019.hkSerialize.CompatTypeParentInfo> 
{
    public hkSerializeCompatTypeParentInfoData(HavokType type, HKLib.hk2019.hkSerialize.CompatTypeParentInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_firstParent":
            case "firstParent":
            {
                if (instance.m_firstParent is null)
                {
                    return true;
                }
                if (instance.m_firstParent is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_firstParent":
            case "firstParent":
            {
                if (value is null)
                {
                    instance.m_firstParent = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkSerialize.CompatTypeParentInfo.Parent castValue)
                {
                    instance.m_firstParent = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
