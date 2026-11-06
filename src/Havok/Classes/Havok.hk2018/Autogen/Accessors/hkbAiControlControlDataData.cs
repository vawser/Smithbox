// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbAiControlControlDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbAiControlControlData>
{
    public hkbAiControlControlDataData(Havok.hk2018.hkbAiControlControlData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blendable":
            case "blendable":
            {
                value = instance.m_blendable;
                return true;
            }
            case "m_nonBlendable":
            case "nonBlendable":
            {
                value = instance.m_nonBlendable;
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
            case "m_blendable":
            case "blendable":
            {
                if (value is not Havok.hk2018.hkbAiControlControlDataBlendable castValue) return false;
                instance.m_blendable = castValue;
                return true;
            }
            case "m_nonBlendable":
            case "nonBlendable":
            {
                if (value is not Havok.hk2018.hkbAiControlControlDataNonBlendable castValue) return false;
                instance.m_nonBlendable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
