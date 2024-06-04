// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkSerialize;

namespace HKLib.Reflection.hk2018;

internal class hkSerializeCompatTypeParentInfoParentData : HavokData<CompatTypeParentInfo.Parent> 
{
    public hkSerializeCompatTypeParentInfoParentData(HavokType type, CompatTypeParentInfo.Parent instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_version":
            case "version":
            {
                if (instance.m_version is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_next":
            case "next":
            {
                if (instance.m_next is null)
                {
                    return true;
                }
                if (instance.m_next is TGet castValue)
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_version":
            case "version":
            {
                if (value is not int castValue) return false;
                instance.m_version = castValue;
                return true;
            }
            case "m_next":
            case "next":
            {
                if (value is null)
                {
                    instance.m_next = default;
                    return true;
                }
                if (value is CompatTypeParentInfo.Parent castValue)
                {
                    instance.m_next = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
