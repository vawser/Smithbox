// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperQuadriplanarInnerTypesLineEqnData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqn>
{
    public hkaiClipperQuadriplanarInnerTypesLineEqnData(Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqn instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_exact":
            case "exact":
            {
                value = instance.m_exact;
                return true;
            }
            case "m_xyApproximate":
            case "xyApproximate":
            {
                value = instance.m_xyApproximate;
                return true;
            }
            case "m_wApproximate":
            case "wApproximate":
            {
                value = instance.m_wApproximate;
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
            case "m_exact":
            case "exact":
            {
                if (value is not Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqnExact castValue) return false;
                instance.m_exact = castValue;
                return true;
            }
            case "m_xyApproximate":
            case "xyApproximate":
            {
                if (value is not Havok.hk2019.hkVector2f castValue) return false;
                instance.m_xyApproximate = castValue;
                return true;
            }
            case "m_wApproximate":
            case "wApproximate":
            {
                if (value is not float castValue) return false;
                instance.m_wApproximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
