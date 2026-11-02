// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBiplanarInnerTypesImplicitPlaneData : HavokData<HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane> 
{
    public hkaiClipperBiplanarInnerTypesImplicitPlaneData(HavokType type, HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xyz":
            case "xyz":
            {
                if (instance.m_xyz is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_w":
            case "w":
            {
                if (instance.m_w is not TGet castValue) return false;
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
            case "m_xyz":
            case "xyz":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpIntVec3_3 castValue) return false;
                instance.m_xyz = castValue;
                return true;
            }
            case "m_w":
            case "w":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_w = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
