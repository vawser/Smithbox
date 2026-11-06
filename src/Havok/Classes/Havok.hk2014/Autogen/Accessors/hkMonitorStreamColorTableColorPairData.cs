// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkMonitorStreamColorTableColorPairData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkMonitorStreamColorTable.ColorPair>
{
    public hkMonitorStreamColorTableColorPairData(Havok.hk2014.hkMonitorStreamColorTable.ColorPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorName":
            case "colorName":
            {
                value = instance.m_colorName;
                return true;
            }
            case "m_color":
            case "color":
            {
                value = instance.m_color;
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
            case "m_colorName":
            case "colorName":
            {
                if (value is null)
                {
                    instance.m_colorName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_colorName = castValue;
                    return true;
                }
                return false;
            }
            case "m_color":
            case "color":
            {
                if (value is not uint castValue) return false;
                instance.m_color = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
