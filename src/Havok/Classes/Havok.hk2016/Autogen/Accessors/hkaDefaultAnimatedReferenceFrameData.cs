// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaDefaultAnimatedReferenceFrameData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaDefaultAnimatedReferenceFrame>
{
    public hkaDefaultAnimatedReferenceFrameData(Havok.hk2016.hkaDefaultAnimatedReferenceFrame instance) : base(instance) { }

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
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_forward":
            case "forward":
            {
                value = instance.m_forward;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_referenceFrameSamples":
            case "referenceFrameSamples":
            {
                value = instance.m_referenceFrameSamples;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_forward":
            case "forward":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_forward = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_referenceFrameSamples":
            case "referenceFrameSamples":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_referenceFrameSamples = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
