// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSetIntFloatPairData : HavokData<hkSetIntFloatPair> 
{
    public hkSetIntFloatPairData(HavokType type, hkSetIntFloatPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elem":
            case "elem":
            {
                if (instance.m_elem is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numElems":
            case "numElems":
            {
                if (instance.m_numElems is not TGet castValue) return false;
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
            case "m_elem":
            case "elem":
            {
                if (value is not List<hkIntRealPair> castValue) return false;
                instance.m_elem = castValue;
                return true;
            }
            case "m_numElems":
            case "numElems":
            {
                if (value is not int castValue) return false;
                instance.m_numElems = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
