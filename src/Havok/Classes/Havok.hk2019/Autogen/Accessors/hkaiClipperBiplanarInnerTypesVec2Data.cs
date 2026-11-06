// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBiplanarInnerTypesVec2Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2>
{
    public hkaiClipperBiplanarInnerTypesVec2Data(Havok.hk2019.hkaiClipper.BiplanarInnerTypes.Vec2 instance) : base(instance) { }

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
            case "m_approximate":
            case "approximate":
            {
                value = instance.m_approximate;
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
                if (value is not Havok.hk2019.hkFpMath.FpFusedRationalVec2_2 castValue) return false;
                instance.m_exact = castValue;
                return true;
            }
            case "m_approximate":
            case "approximate":
            {
                if (value is not Havok.hk2019.hkVector2f castValue) return false;
                instance.m_approximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
