// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMonitorStreamColorTableData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMonitorStreamColorTable>
{
    public hkMonitorStreamColorTableData(Havok.hk2019.hkMonitorStreamColorTable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorPairs":
            case "colorPairs":
            {
                value = instance.m_colorPairs;
                return true;
            }
            case "m_defaultColor":
            case "defaultColor":
            {
                value = instance.m_defaultColor;
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
            case "m_colorPairs":
            case "colorPairs":
            {
                if (value is not List<Havok.hk2019.hkMonitorStreamColorTable.ColorPair> castValue) return false;
                instance.m_colorPairs = castValue;
                return true;
            }
            case "m_defaultColor":
            case "defaultColor":
            {
                if (value is not uint castValue) return false;
                instance.m_defaultColor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
