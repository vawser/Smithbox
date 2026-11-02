// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBiplanarInnerTypesBasisData : HavokData<HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.Basis> 
{
    public hkaiClipperBiplanarInnerTypesBasisData(HavokType type, HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.Basis instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_facePlane":
            case "facePlane":
            {
                if (instance.m_facePlane is not TGet castValue) return false;
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
            case "m_facePlane":
            case "facePlane":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.FacePlane castValue) return false;
                instance.m_facePlane = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
