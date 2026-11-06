// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBasisQuadriplanarInnerTypesLineEqnData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.LineEqn>
{
    public hkaiClipperBasisQuadriplanarInnerTypesLineEqnData(Havok.hk2019.hkaiClipper.BasisQuadriplanarInnerTypes.LineEqn instance) : base(instance) { }

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
            case "m_uvApproximate":
            case "uvApproximate":
            {
                value = instance.m_uvApproximate;
                return true;
            }
            case "m_uvwApproximate":
            case "uvwApproximate":
            {
                value = instance.m_uvwApproximate;
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
            case "m_uvApproximate":
            case "uvApproximate":
            {
                if (value is not Havok.hk2019.hkVector2f castValue) return false;
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
