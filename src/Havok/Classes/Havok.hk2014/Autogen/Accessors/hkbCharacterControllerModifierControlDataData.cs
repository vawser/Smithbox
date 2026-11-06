// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterControllerModifierControlDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterControllerModifierControlData>
{
    public hkbCharacterControllerModifierControlDataData(Havok.hk2014.hkbCharacterControllerModifierControlData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_verticalGain":
            case "verticalGain":
            {
                value = instance.m_verticalGain;
                return true;
            }
            case "m_horizontalCatchUpGain":
            case "horizontalCatchUpGain":
            {
                value = instance.m_horizontalCatchUpGain;
                return true;
            }
            case "m_maxVerticalSeparation":
            case "maxVerticalSeparation":
            {
                value = instance.m_maxVerticalSeparation;
                return true;
            }
            case "m_maxHorizontalSeparation":
            case "maxHorizontalSeparation":
            {
                value = instance.m_maxHorizontalSeparation;
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
