// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkMonitorStreamColorTableData : HavokData<HKLib.hk2019.hkMonitorStreamColorTable> 
{
    public hkMonitorStreamColorTableData(HavokType type, HKLib.hk2019.hkMonitorStreamColorTable instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorPairs":
            case "colorPairs":
            {
                if (instance.m_colorPairs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultColor":
            case "defaultColor":
            {
                if (instance.m_defaultColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_colorPairs":
            case "colorPairs":
            {
                if (value is not List<HKLib.hk2019.hkMonitorStreamColorTable.ColorPair> castValue) return false;
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
