// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkFpMathFpFusedRationalVec2_2Data : HavokData<HKLib.hk2019.hkFpMath.FpFusedRationalVec2_2> 
{
    public hkFpMathFpFusedRationalVec2_2Data(HavokType type, HKLib.hk2019.hkFpMath.FpFusedRationalVec2_2 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numerator":
            case "numerator":
            {
                if (instance.m_numerator is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_denominator":
            case "denominator":
            {
                if (instance.m_denominator is not TGet castValue) return false;
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
            case "m_numerator":
            case "numerator":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpIntVec2_4 castValue) return false;
                instance.m_numerator = castValue;
                return true;
            }
            case "m_denominator":
            case "denominator":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_denominator = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
