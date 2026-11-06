// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpPropertyData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpProperty>
{
    public hkpPropertyData(Havok.hk55.hkpProperty instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_key":
            case "key":
            {
                value = instance.m_key;
                return true;
            }
            case "m_alignmentPadding":
            case "alignmentPadding":
            {
                value = instance.m_alignmentPadding;
                return true;
            }
            case "m_value":
            case "value":
            {
                value = instance.m_value;
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
            case "m_key":
            case "key":
            {
                if (value is not uint castValue) return false;
                instance.m_key = castValue;
                return true;
            }
            case "m_alignmentPadding":
            case "alignmentPadding":
            {
                if (value is not uint castValue) return false;
                instance.m_alignmentPadding = castValue;
                return true;
            }
            case "m_value":
            case "value":
            {
                if (value is not Havok.hk55.hkpPropertyValue castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
