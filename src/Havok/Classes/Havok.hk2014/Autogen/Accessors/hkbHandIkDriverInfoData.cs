// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbHandIkDriverInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbHandIkDriverInfo>
{
    public hkbHandIkDriverInfoData(Havok.hk2014.hkbHandIkDriverInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hands":
            case "hands":
            {
                value = instance.m_hands;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                value = instance.m_fadeInOutCurve;
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
            case "m_hands":
            case "hands":
            {
                if (value is not List<Havok.hk2014.hkbHandIkDriverInfo.Hand> castValue) return false;
                instance.m_hands = castValue;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                if (value is Havok.hk2014.hkbBlendCurveUtils.BlendCurve castValue)
                {
                    instance.m_fadeInOutCurve = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadeInOutCurve = (Havok.hk2014.hkbBlendCurveUtils.BlendCurve)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
