// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBasisQuadriplanarInnerTypesVec2Data : HavokData<HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Vec2> 
{
    public hkaiClipperBasisQuadriplanarInnerTypesVec2Data(HavokType type, HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Vec2 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xyExact":
            case "xyExact":
            {
                if (instance.m_xyExact is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uvApproximate":
            case "uvApproximate":
            {
                if (instance.m_uvApproximate is not TGet castValue) return false;
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
            case "m_xyExact":
            case "xyExact":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpFusedRationalVec2 castValue) return false;
                instance.m_xyExact = castValue;
                return true;
            }
            case "m_uvApproximate":
            case "uvApproximate":
            {
                if (value is not HKLib.hk2019.hkVector2f castValue) return false;
                instance.m_uvApproximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
