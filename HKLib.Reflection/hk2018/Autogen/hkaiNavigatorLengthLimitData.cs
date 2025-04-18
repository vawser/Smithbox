// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavigatorLengthLimitData : HavokData<hkaiNavigatorLengthLimit> 
{
    public hkaiNavigatorLengthLimitData(HavokType type, hkaiNavigatorLengthLimit instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_baseLength":
            case "baseLength":
            {
                if (instance.m_baseLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lengthFactor":
            case "lengthFactor":
            {
                if (instance.m_lengthFactor is not TGet castValue) return false;
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
            case "m_baseLength":
            case "baseLength":
            {
                if (value is not float castValue) return false;
                instance.m_baseLength = castValue;
                return true;
            }
            case "m_lengthFactor":
            case "lengthFactor":
            {
                if (value is not float castValue) return false;
                instance.m_lengthFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
