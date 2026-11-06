// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBasisQuadriplanarInnerTypesVec2Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Vec2>
{
    public hkaiClipperBasisQuadriplanarInnerTypesVec2Data(Havok.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.Vec2 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xyExact":
            case "xyExact":
            {
                value = instance.m_xyExact;
                return true;
            }
            case "m_uvApproximate":
            case "uvApproximate":
            {
                value = instance.m_uvApproximate;
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
            case "m_xyExact":
            case "xyExact":
            {
                if (value is not Havok.hk2019.hkFpMath.FpFusedRationalVec2 castValue) return false;
                instance.m_xyExact = castValue;
                return true;
            }
            case "m_uvApproximate":
            case "uvApproximate":
            {
                if (value is not Havok.hk2019.hkVector2f castValue) return false;
                instance.m_uvApproximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
