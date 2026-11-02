// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiClipperQuadriplanarInnerTypesLineEqnExactData : HavokData<HKLib.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqnExact> 
{
    public hkaiClipperQuadriplanarInnerTypesLineEqnExactData(HavokType type, HKLib.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqnExact instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xy":
            case "xy":
            {
                if (instance.m_xy is not TGet castValue) return false;
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
            case "m_xy":
            case "xy":
            {
                if (value is not HKLib.hk2019.hkFpMath.FpIntVec2 castValue) return false;
                instance.m_xy = castValue;
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
