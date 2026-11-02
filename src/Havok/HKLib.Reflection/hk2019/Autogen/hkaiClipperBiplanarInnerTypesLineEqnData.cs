// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBiplanarInnerTypesLineEqnData : HavokData<HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.LineEqn> 
{
    public hkaiClipperBiplanarInnerTypesLineEqnData(HavokType type, HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.LineEqn instance) : base(type, instance) {}

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
            case "m_approximateUV":
            case "approximateUV":
            {
                if (instance.m_approximateUV is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_approximateW":
            case "approximateW":
            {
                if (instance.m_approximateW is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.ProjectedPlane castValue) return false;
                instance.m_exact = castValue;
                return true;
            }
            case "m_approximateUV":
            case "approximateUV":
            {
                if (value is not HKLib.hk2019.hkVector2f castValue) return false;
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
