// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterControllerModifierControlDataData : HavokData<hkbCharacterControllerModifierControlData> 
{
    public hkbCharacterControllerModifierControlDataData(HavokType type, hkbCharacterControllerModifierControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_verticalGain":
            case "verticalGain":
            {
                if (instance.m_verticalGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_horizontalCatchUpGain":
            case "horizontalCatchUpGain":
            {
                if (instance.m_horizontalCatchUpGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxVerticalSeparation":
            case "maxVerticalSeparation":
            {
                if (instance.m_maxVerticalSeparation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxHorizontalSeparation":
            case "maxHorizontalSeparation":
            {
                if (instance.m_maxHorizontalSeparation is not TGet castValue) return false;
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
            case "m_verticalGain":
            case "verticalGain":
            {
                if (value is not float castValue) return false;
                instance.m_verticalGain = castValue;
                return true;
            }
            case "m_horizontalCatchUpGain":
            case "horizontalCatchUpGain":
            {
                if (value is not float castValue) return false;
                instance.m_horizontalCatchUpGain = castValue;
                return true;
            }
            case "m_maxVerticalSeparation":
            case "maxVerticalSeparation":
            {
                if (value is not float castValue) return false;
                instance.m_maxVerticalSeparation = castValue;
                return true;
            }
            case "m_maxHorizontalSeparation":
            case "maxHorizontalSeparation":
            {
                if (value is not float castValue) return false;
                instance.m_maxHorizontalSeparation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
