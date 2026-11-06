// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbAiControlControlDataNonBlendableData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbAiControlControlDataNonBlendable>
{
    public hkbAiControlControlDataNonBlendableData(Havok.hk2014.hkbAiControlControlDataNonBlendable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canControl":
            case "canControl":
            {
                value = instance.m_canControl;
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
            case "m_canControl":
            case "canControl":
            {
                if (value is not bool castValue) return false;
                instance.m_canControl = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
