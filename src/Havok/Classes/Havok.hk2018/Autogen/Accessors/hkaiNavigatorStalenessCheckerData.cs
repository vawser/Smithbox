// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavigatorStalenessCheckerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavigatorStalenessChecker>
{
    public hkaiNavigatorStalenessCheckerData(Havok.hk2018.hkaiNavigatorStalenessChecker instance) : base(instance) { }

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
            case "m_navigator":
            case "navigator":
            {
                value = instance.m_navigator;
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
            case "m_pathFaceKeys":
            case "pathFaceKeys":
            {
                value = instance.m_pathFaceKeys;
                return true;
            }
            case "m_trailingEdge":
            case "trailingEdge":
            {
                value = instance.m_trailingEdge;
                return true;
            }
            case "m_state":
            case "state":
            {
                value = instance.m_state;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                value = instance.m_pathRequest;
                return true;
            }
            case "m_removedGoalIndices":
            case "removedGoalIndices":
            {
                value = instance.m_removedGoalIndices;
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
            case "m_navigator":
            case "navigator":
            {
                if (value is null)
                {
                    instance.m_navigator = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavigator castValue)
                {
                    instance.m_navigator = castValue;
                    return true;
                }
                return false;
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
            case "m_pathFaceKeys":
            case "pathFaceKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_pathFaceKeys = castValue;
                return true;
            }
            case "m_trailingEdge":
            case "trailingEdge":
            {
                if (value is not int castValue) return false;
                instance.m_trailingEdge = castValue;
                return true;
            }
            case "m_state":
            case "state":
            {
                if (value is not Havok.hk2018.hkAtomic.Variable<Havok.hk2018.hkaiNavigatorStalenessChecker.State> castValue) return false;
                instance.m_state = castValue;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                if (value is null)
                {
                    instance.m_pathRequest = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavigator.PathRequest castValue)
                {
                    instance.m_pathRequest = castValue;
                    return true;
                }
                return false;
            }
            case "m_removedGoalIndices":
            case "removedGoalIndices":
            {
                if (value is not List<int> castValue) return false;
                instance.m_removedGoalIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
