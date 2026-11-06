// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMonitorStreamColorTableData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMonitorStreamColorTable>
{
    public hkMonitorStreamColorTableData(Havok.hk2018.hkMonitorStreamColorTable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_colorPairs":
            case "colorPairs":
            {
                if (value is not List<Havok.hk2018.hkMonitorStreamColorTable.ColorPair> castValue) return false;
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
