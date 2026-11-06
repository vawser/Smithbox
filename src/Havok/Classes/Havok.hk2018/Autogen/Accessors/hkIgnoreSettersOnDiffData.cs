// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkIgnoreSettersOnDiffData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.IgnoreSettersOnDiff>
{
    public hkIgnoreSettersOnDiffData(Havok.hk2018.hk.IgnoreSettersOnDiff instance) : base(instance) { }

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
                if (value is not bool castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
