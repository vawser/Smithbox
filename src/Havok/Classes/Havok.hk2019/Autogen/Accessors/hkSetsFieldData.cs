// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSetsFieldData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.SetsField>
{
    public hkSetsFieldData(Havok.hk2019.hk.SetsField instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_value":
            case "value":
            {
                if (value is not ulong castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
