// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbBehaviorGraphStringDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbBehaviorGraphStringData>
{
    public hkbBehaviorGraphStringDataData(Havok.hk2016.hkbBehaviorGraphStringData instance) : base(instance) { }

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
            case "m_eventNames":
            case "eventNames":
            {
                value = instance.m_eventNames;
                return true;
            }
            case "m_attributeNames":
            case "attributeNames":
            {
                value = instance.m_attributeNames;
                return true;
            }
            case "m_variableNames":
            case "variableNames":
            {
                value = instance.m_variableNames;
                return true;
            }
            case "m_characterPropertyNames":
            case "characterPropertyNames":
            {
                value = instance.m_characterPropertyNames;
                return true;
            }
            case "m_animationNames":
            case "animationNames":
            {
                value = instance.m_animationNames;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_eventNames":
            case "eventNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_eventNames = castValue;
                return true;
            }
            case "m_attributeNames":
            case "attributeNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_attributeNames = castValue;
                return true;
            }
            case "m_variableNames":
            case "variableNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_variableNames = castValue;
                return true;
            }
            case "m_characterPropertyNames":
            case "characterPropertyNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_characterPropertyNames = castValue;
                return true;
            }
            case "m_animationNames":
            case "animationNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_animationNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
