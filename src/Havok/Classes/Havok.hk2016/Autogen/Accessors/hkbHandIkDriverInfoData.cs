// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbHandIkDriverInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbHandIkDriverInfo>
{
    public hkbHandIkDriverInfoData(Havok.hk2016.hkbHandIkDriverInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_hands":
            case "hands":
            {
                if (value is not List<Havok.hk2016.hkbHandIkDriverInfo.Hand> castValue) return false;
                instance.m_hands = castValue;
                return true;
            }
            case "m_fadeInOutCurve":
            case "fadeInOutCurve":
            {
                if (value is Havok.hk2016.hkbBlendCurveUtils.BlendCurve castValue)
                {
                    instance.m_fadeInOutCurve = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadeInOutCurve = (Havok.hk2016.hkbBlendCurveUtils.BlendCurve)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
