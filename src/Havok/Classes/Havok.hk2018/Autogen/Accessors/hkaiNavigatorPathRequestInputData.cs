// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavigatorPathRequestInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavigator.PathRequestInput>
{
    public hkaiNavigatorPathRequestInputData(Havok.hk2018.hkaiNavigator.PathRequestInput instance) : base(instance) { }

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
            case "m_startPoint":
            case "startPoint":
            {
                value = instance.m_startPoint;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                value = instance.m_goals;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                value = instance.m_settings;
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
            case "m_startPoint":
            case "startPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_startPoint = castValue;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                if (value is not List<Havok.hk2018.hkaiNavigator.Goal> castValue) return false;
                instance.m_goals = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is null)
                {
                    instance.m_settings = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavigator.NavigatorSettings castValue)
                {
                    instance.m_settings = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
