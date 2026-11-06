// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCameraShakeEventPayloadData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCameraShakeEventPayload>
{
    public hkbCameraShakeEventPayloadData(Havok.hk2018.hkbCameraShakeEventPayload instance) : base(instance) { }

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
            case "m_amplitude":
            case "amplitude":
            {
                value = instance.m_amplitude;
                return true;
            }
            case "m_halfLife":
            case "halfLife":
            {
                value = instance.m_halfLife;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_amplitude":
            case "amplitude":
            {
                if (value is not float castValue) return false;
                instance.m_amplitude = castValue;
                return true;
            }
            case "m_halfLife":
            case "halfLife":
            {
                if (value is not float castValue) return false;
                instance.m_halfLife = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
