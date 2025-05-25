// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavigatorPathRequestInputData : HavokData<hkaiNavigator.PathRequestInput> 
{
    public hkaiNavigatorPathRequestInputData(HavokType type, hkaiNavigator.PathRequestInput instance) : base(type, instance) {}

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
            case "m_startPoint":
            case "startPoint":
            {
                if (instance.m_startPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                if (instance.m_goals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (instance.m_settings is null)
                {
                    return true;
                }
                if (instance.m_settings is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
                if (value is not List<hkaiNavigator.Goal> castValue) return false;
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
                if (value is hkaiNavigator.NavigatorSettings castValue)
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
