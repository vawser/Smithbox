// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbStringConditionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbStringCondition>
{
    public hkbStringConditionData(Havok.hk2018.hkbStringCondition instance) : base(instance) { }

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
            case "m_conditionString":
            case "conditionString":
            {
                value = instance.m_conditionString;
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
            case "m_conditionString":
            case "conditionString":
            {
                if (value is null)
                {
                    instance.m_conditionString = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_conditionString = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
