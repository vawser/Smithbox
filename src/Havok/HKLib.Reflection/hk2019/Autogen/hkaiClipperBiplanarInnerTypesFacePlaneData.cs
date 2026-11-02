// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBiplanarInnerTypesFacePlaneData : HavokData<HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.FacePlane> 
{
    public hkaiClipperBiplanarInnerTypesFacePlaneData(HavokType type, HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.FacePlane instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                if (instance.m_a is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ab":
            case "ab":
            {
                if (instance.m_ab is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ac":
            case "ac":
            {
                if (instance.m_ac is not TGet castValue) return false;
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
            case "m_a":
            case "a":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpIntVec3_4 castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_ab":
            case "ab":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpIntVec3 castValue) return false;
                instance.m_ab = castValue;
                return true;
            }
            case "m_ac":
            case "ac":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpIntVec3 castValue) return false;
                instance.m_ac = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
