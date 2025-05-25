// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkBitFieldStorageData<Storage> : HavokData<hkBitFieldStorage<Storage>> 
{
    public hkBitFieldStorageData(HavokType type, hkBitFieldStorage<Storage> instance) : base(type, instance) {}

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
            case "m_numBits":
            case "numBits":
            {
                if (instance.m_numBits is not TGet castValue) return false;
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
            case "m_numBits":
            case "numBits":
            {
                if (value is not int castValue) return false;
                instance.m_numBits = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
