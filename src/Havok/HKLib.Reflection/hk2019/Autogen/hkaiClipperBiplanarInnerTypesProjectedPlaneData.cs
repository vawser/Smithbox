// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperBiplanarInnerTypesProjectedPlaneData : HavokData<HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.ProjectedPlane> 
{
    public hkaiClipperBiplanarInnerTypesProjectedPlaneData(HavokType type, HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.ProjectedPlane instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_uv":
            case "uv":
            {
                if (instance.m_uv is not TGet castValue) return false;
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
            case "m_uv":
            case "uv":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpIntVec2_3 castValue) return false;
                instance.m_uv = castValue;
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
