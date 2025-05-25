// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkResultData : HavokData<hkResult> 
{
    public hkResultData(HavokType type, hkResult instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_code":
            case "code":
            {
                if (instance.m_code is not TGet castValue) return false;
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
            case "m_code":
            case "code":
            {
                if (value is not int castValue) return false;
                instance.m_code = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
