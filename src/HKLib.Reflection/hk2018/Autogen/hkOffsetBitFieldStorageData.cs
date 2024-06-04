// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkOffsetBitFieldStorageData<Storage> : HavokData<hkOffsetBitFieldStorage<Storage>> 
{
    public hkOffsetBitFieldStorageData(HavokType type, hkOffsetBitFieldStorage<Storage> instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_words":
            case "words":
            {
                if (instance.m_words is null)
                {
                    return true;
                }
                if (instance.m_words is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_offset":
            case "offset":
            {
                if (instance.m_offset is not TGet castValue) return false;
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
            case "m_words":
            case "words":
            {
                if (value is null)
                {
                    instance.m_words = default;
                    return true;
                }
                if (value is Storage castValue)
                {
                    instance.m_words = castValue;
                    return true;
                }
                return false;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not int castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
