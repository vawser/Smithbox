// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperQuadriplanarInnerTypesVec2Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.Vec2>
{
    public hkaiClipperQuadriplanarInnerTypesVec2Data(Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.Vec2 instance) : base(instance) { }

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
            case "m_xyApproximate":
            case "xyApproximate":
            {
                value = instance.m_xyApproximate;
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
            case "m_xyApproximate":
            case "xyApproximate":
            {
                if (value is not Havok.hk2019.hkVector2f castValue) return false;
                instance.m_xyApproximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
