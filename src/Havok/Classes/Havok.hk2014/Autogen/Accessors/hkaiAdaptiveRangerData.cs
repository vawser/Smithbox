// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiAdaptiveRangerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiAdaptiveRanger>
{
    public hkaiAdaptiveRangerData(Havok.hk2014.hkaiAdaptiveRanger instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_curRange":
            case "curRange":
            {
                value = instance.m_curRange;
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
            case "m_curRange":
            case "curRange":
            {
                if (value is not float castValue) return false;
                instance.m_curRange = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
