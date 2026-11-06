// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCameraShakeEventPayloadData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCameraShakeEventPayload>
{
    public hkbCameraShakeEventPayloadData(Havok.hk2014.hkbCameraShakeEventPayload instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
