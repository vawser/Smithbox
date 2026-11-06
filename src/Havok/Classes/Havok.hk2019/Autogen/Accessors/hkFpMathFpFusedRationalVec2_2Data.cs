// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkFpMathFpFusedRationalVec2_2Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkFpMath.FpFusedRationalVec2_2>
{
    public hkFpMathFpFusedRationalVec2_2Data(Havok.hk2019.hkFpMath.FpFusedRationalVec2_2 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numerator":
            case "numerator":
            {
                value = instance.m_numerator;
                return true;
            }
            case "m_denominator":
            case "denominator":
            {
                value = instance.m_denominator;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_numerator":
            case "numerator":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec2_4 castValue) return false;
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
