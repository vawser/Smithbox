// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMonitorStreamColorTableData : HavokData<hkMonitorStreamColorTable> 
{
    public hkMonitorStreamColorTableData(HavokType type, hkMonitorStreamColorTable instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_colorPairs":
            case "colorPairs":
            {
                if (value is not List<hkMonitorStreamColorTable.ColorPair> castValue) return false;
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
