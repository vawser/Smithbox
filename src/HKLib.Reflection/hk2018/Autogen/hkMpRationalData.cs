// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMpRationalData : HavokData<hkMpRational> 
{
    public hkMpRationalData(HavokType type, hkMpRational instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_num":
            case "num":
            {
                if (instance.m_num is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_den":
            case "den":
            {
                if (instance.m_den is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_signed":
            case "signed":
            {
                if (instance.m_signed is not TGet castValue) return false;
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
            case "m_num":
            case "num":
            {
                if (value is not hkMpUint castValue) return false;
                instance.m_num = castValue;
                return true;
            }
            case "m_den":
            case "den":
            {
                if (value is not hkMpUint castValue) return false;
                instance.m_den = castValue;
                return true;
            }
            case "m_signed":
            case "signed":
            {
                if (value is not bool castValue) return false;
                instance.m_signed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
