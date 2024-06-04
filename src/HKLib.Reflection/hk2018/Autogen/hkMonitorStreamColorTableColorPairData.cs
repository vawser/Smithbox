// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMonitorStreamColorTableColorPairData : HavokData<hkMonitorStreamColorTable.ColorPair> 
{
    public hkMonitorStreamColorTableColorPairData(HavokType type, hkMonitorStreamColorTable.ColorPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorName":
            case "colorName":
            {
                if (instance.m_colorName is null)
                {
                    return true;
                }
                if (instance.m_colorName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_color":
            case "color":
            {
                if (instance.m_color is not TGet castValue) return false;
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
                if (value is not Color castValue) return false;
                instance.m_color = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
