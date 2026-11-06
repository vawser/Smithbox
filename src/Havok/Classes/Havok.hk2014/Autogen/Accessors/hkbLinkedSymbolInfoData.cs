// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbLinkedSymbolInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbLinkedSymbolInfo>
{
    public hkbLinkedSymbolInfoData(Havok.hk2014.hkbLinkedSymbolInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_eventNames":
            case "eventNames":
            {
                value = instance.m_eventNames;
                return true;
            }
            case "m_variableNames":
            case "variableNames":
            {
                value = instance.m_variableNames;
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
            case "m_eventNames":
            case "eventNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_eventNames = castValue;
                return true;
            }
            case "m_variableNames":
            case "variableNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_variableNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
