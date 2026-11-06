// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBiplanarInnerTypesFacePlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BiplanarInnerTypes.FacePlane>
{
    public hkaiClipperBiplanarInnerTypesFacePlaneData(Havok.hk2019.hkaiClipper.BiplanarInnerTypes.FacePlane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                value = instance.m_a;
                return true;
            }
            case "m_ab":
            case "ab":
            {
                value = instance.m_ab;
                return true;
            }
            case "m_ac":
            case "ac":
            {
                value = instance.m_ac;
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
            case "m_a":
            case "a":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec3_4 castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_ab":
            case "ab":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec3 castValue) return false;
                instance.m_ab = castValue;
                return true;
            }
            case "m_ac":
            case "ac":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec3 castValue) return false;
                instance.m_ac = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
