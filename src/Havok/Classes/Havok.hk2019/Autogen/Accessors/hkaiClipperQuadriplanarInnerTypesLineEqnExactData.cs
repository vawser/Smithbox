// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperQuadriplanarInnerTypesLineEqnExactData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqnExact>
{
    public hkaiClipperQuadriplanarInnerTypesLineEqnExactData(Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqnExact instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xy":
            case "xy":
            {
                value = instance.m_xy;
                return true;
            }
            case "m_w":
            case "w":
            {
                value = instance.m_w;
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
            case "m_xy":
            case "xy":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec2 castValue) return false;
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
