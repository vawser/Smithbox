// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBiplanarInnerTypesVec2Data : HavokData<HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2> 
{
    public hkaiClipperBiplanarInnerTypesVec2Data(HavokType type, HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2 instance) : base(type, instance) {}

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
            case "m_approximate":
            case "approximate":
            {
                if (instance.m_approximate is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkFpMath.FpFusedRationalVec2_2 castValue) return false;
                instance.m_exact = castValue;
                return true;
            }
            case "m_approximate":
            case "approximate":
            {
                if (value is not HKLib.hk2019.hkVector2f castValue) return false;
                instance.m_approximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
