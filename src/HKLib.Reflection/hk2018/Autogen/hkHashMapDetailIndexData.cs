// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkHashMapDetail;
using Index = HKLib.hk2018.hkHashMapDetail.Index;

namespace HKLib.Reflection.hk2018;

internal class hkHashMapDetailIndexData : HavokData<Index> 
{
    public hkHashMapDetailIndexData(HavokType type, Index instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entries":
            case "entries":
            {
                if (instance.m_entries is null)
                {
                    return true;
                }
                if (instance.m_entries is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_hashMod":
            case "hashMod":
            {
                if (instance.m_hashMod is not TGet castValue) return false;
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
            case "m_entries":
            case "entries":
            {
                if (value is null)
                {
                    instance.m_entries = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_entries = castValue;
                    return true;
                }
                return false;
            }
            case "m_hashMod":
            case "hashMod":
            {
                if (value is not int castValue) return false;
                instance.m_hashMod = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
