// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbAiControlControlDataBlendableData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbAiControlControlDataBlendable>
{
    public hkbAiControlControlDataBlendableData(Havok.hk2014.hkbAiControlControlDataBlendable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_desiredSpeed":
            case "desiredSpeed":
            {
                value = instance.m_desiredSpeed;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                value = instance.m_maximumSpeed;
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
            case "m_desiredSpeed":
            case "desiredSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_desiredSpeed = castValue;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maximumSpeed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
