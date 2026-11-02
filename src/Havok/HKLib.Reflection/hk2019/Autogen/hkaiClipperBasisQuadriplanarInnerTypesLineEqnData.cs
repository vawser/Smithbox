// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBasisQuadriplanarInnerTypesLineEqnData : HavokData<HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.LineEqn> 
{
    public hkaiClipperBasisQuadriplanarInnerTypesLineEqnData(HavokType type, HKLib.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.LineEqn instance) : base(type, instance) {}

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
            case "m_uvApproximate":
            case "uvApproximate":
            {
                if (instance.m_uvApproximate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uvwApproximate":
            case "uvwApproximate":
            {
                if (instance.m_uvwApproximate is not TGet castValue) return false;
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
            case "m_uvApproximate":
            case "uvApproximate":
            {
                if (value is not HKLib.hk2019.hkVector2f castValue) return false;
                instance.m_uvApproximate = castValue;
                return true;
            }
            case "m_uvwApproximate":
            case "uvwApproximate":
            {
                if (value is not float castValue) return false;
                instance.m_uvwApproximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
