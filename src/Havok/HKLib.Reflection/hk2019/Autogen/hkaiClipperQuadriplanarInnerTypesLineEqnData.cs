// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperQuadriplanarInnerTypesLineEqnData : HavokData<HKLib.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqn> 
{
    public hkaiClipperQuadriplanarInnerTypesLineEqnData(HavokType type, HKLib.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqn instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_exact":
            case "exact":
            {
                if (instance.m_exact is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_xyApproximate":
            case "xyApproximate":
            {
                if (instance.m_xyApproximate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wApproximate":
            case "wApproximate":
            {
                if (instance.m_wApproximate is not TGet castValue) return false;
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
            case "m_exact":
            case "exact":
            {
                if (value is not HKLib.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqnExact castValue) return false;
                instance.m_exact = castValue;
                return true;
            }
            case "m_xyApproximate":
            case "xyApproximate":
            {
                if (value is not HKLib.hk2019.hkVector2f castValue) return false;
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
