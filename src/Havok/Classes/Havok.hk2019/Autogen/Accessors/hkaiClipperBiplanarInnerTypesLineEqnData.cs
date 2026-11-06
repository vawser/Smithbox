// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBiplanarInnerTypesLineEqnData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BiplanarInnerTypes.LineEqn>
{
    public hkaiClipperBiplanarInnerTypesLineEqnData(Havok.hk2019.hkaiClipper.BiplanarInnerTypes.LineEqn instance) : base(instance) { }

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
            case "m_approximateUV":
            case "approximateUV":
            {
                value = instance.m_approximateUV;
                return true;
            }
            case "m_approximateW":
            case "approximateW":
            {
                value = instance.m_approximateW;
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
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.ProjectedPlane castValue) return false;
                instance.m_exact = castValue;
                return true;
            }
            case "m_approximateUV":
            case "approximateUV":
            {
                if (value is not Havok.hk2019.hkVector2f castValue) return false;
                instance.m_approximateUV = castValue;
                return true;
            }
            case "m_approximateW":
            case "approximateW":
            {
                if (value is not float castValue) return false;
                instance.m_approximateW = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
