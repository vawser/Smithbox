// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiAgentTraversalInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiAgentTraversalInfo>
{
    public hkaiAgentTraversalInfoData(Havok.hk2019.hkaiAgentTraversalInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_diameter":
            case "diameter":
            {
                value = instance.m_diameter;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                value = instance.m_filterInfo;
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
            case "m_diameter":
            case "diameter":
            {
                if (value is not float castValue) return false;
                instance.m_diameter = castValue;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
