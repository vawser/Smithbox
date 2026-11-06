// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class PolygonSetup2dData : Havok.Formats.HavokFieldAccess<Havok.hk2019.PolygonSetup2d>
{
    public PolygonSetup2dData(Havok.hk2019.PolygonSetup2d instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pad":
            case "pad":
            {
                value = instance.m_pad;
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
            case "m_pad":
            case "pad":
            {
                if (value is not int castValue) return false;
                instance.m_pad = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
