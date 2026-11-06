// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbClipGeneratorEchoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbClipGenerator.Echo>
{
    public hkbClipGeneratorEchoData(Havok.hk2018.hkbClipGenerator.Echo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offsetLocalTime":
            case "offsetLocalTime":
            {
                value = instance.m_offsetLocalTime;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_dwdt":
            case "dwdt":
            {
                value = instance.m_dwdt;
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
            case "m_offsetLocalTime":
            case "offsetLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_offsetLocalTime = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_dwdt":
            case "dwdt":
            {
                if (value is not float castValue) return false;
                instance.m_dwdt = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
