// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavigatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavigator>
{
    public hkaiNavigatorData(Havok.hk2018.hkaiNavigator instance) : base(instance) { }

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
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_currentGoals":
            case "currentGoals":
            {
                value = instance.m_currentGoals;
                return true;
            }
            case "m_rawPosition":
            case "rawPosition":
            {
                value = instance.m_rawPosition;
                return true;
            }
            case "m_hasPosition":
            case "hasPosition":
            {
                value = instance.m_hasPosition;
                return true;
            }
            case "m_lastIdealDirection":
            case "lastIdealDirection":
            {
                value = instance.m_lastIdealDirection;
                return true;
            }
            case "m_lastIdealDirectionIsLocalSpace":
            case "lastIdealDirectionIsLocalSpace":
            {
                value = instance.m_lastIdealDirectionIsLocalSpace;
                return true;
            }
            case "m_hasLastIdealDirection":
            case "hasLastIdealDirection":
            {
                value = instance.m_hasLastIdealDirection;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                value = instance.m_settings;
                return true;
            }
            case "m_world":
            case "world":
            {
                value = instance.m_world;
                return true;
            }
            case "m_lastNavigationOutput":
            case "lastNavigationOutput":
            {
                value = instance.m_lastNavigationOutput;
                return true;
            }
            case "m_cornerPredictorInitInfo":
            case "cornerPredictorInitInfo":
            {
                value = instance.m_cornerPredictorInitInfo;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                value = instance.m_pathRequest;
                return true;
            }
            case "m_goalReached":
            case "goalReached":
            {
                value = instance.m_goalReached;
                return true;
            }
            case "m_edgePath":
            case "edgePath":
            {
                value = instance.m_edgePath;
                return true;
            }
            case "m_stalenessChecker":
            case "stalenessChecker":
            {
                value = instance.m_stalenessChecker;
                return true;
            }
            case "m_traversalState":
            case "traversalState":
            {
                value = instance.m_traversalState;
                return true;
            }
            case "m_worldIndex":
            case "worldIndex":
            {
                value = instance.m_worldIndex;
                return true;
            }
            case "m_pathRequestQueueId":
            case "pathRequestQueueId":
            {
                value = instance.m_pathRequestQueueId;
                return true;
            }
            case "m_acceptablePathQualities":
            case "acceptablePathQualities":
            {
                value = instance.m_acceptablePathQualities;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_currentGoals":
            case "currentGoals":
            {
                if (value is not List<Havok.hk2018.hkaiNavigator.Goal> castValue) return false;
                instance.m_currentGoals = castValue;
                return true;
            }
            case "m_rawPosition":
            case "rawPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rawPosition = castValue;
                return true;
            }
            case "m_hasPosition":
            case "hasPosition":
            {
                if (value is not bool castValue) return false;
                instance.m_hasPosition = castValue;
                return true;
            }
            case "m_lastIdealDirection":
            case "lastIdealDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lastIdealDirection = castValue;
                return true;
            }
            case "m_lastIdealDirectionIsLocalSpace":
            case "lastIdealDirectionIsLocalSpace":
            {
                if (value is not bool castValue) return false;
                instance.m_lastIdealDirectionIsLocalSpace = castValue;
                return true;
            }
            case "m_hasLastIdealDirection":
            case "hasLastIdealDirection":
            {
                if (value is not bool castValue) return false;
                instance.m_hasLastIdealDirection = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not Havok.hk2018.hkaiNavigator.NavigatorSettings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            case "m_world":
            case "world":
            {
                if (value is null)
                {
                    instance.m_world = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiWorld castValue)
                {
                    instance.m_world = castValue;
                    return true;
                }
                return false;
            }
            case "m_lastNavigationOutput":
            case "lastNavigationOutput":
            {
                if (value is not Havok.hk2018.hkaiNavigator.NavigationOutput castValue) return false;
                instance.m_lastNavigationOutput = castValue;
                return true;
            }
            case "m_cornerPredictorInitInfo":
            case "cornerPredictorInitInfo":
            {
                if (value is not Havok.hk2018.hkaiNavigator.CornerPredictorInitInfo castValue) return false;
                instance.m_cornerPredictorInitInfo = castValue;
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
            case "m_goalReached":
            case "goalReached":
            {
                if (value is not bool castValue) return false;
                instance.m_goalReached = castValue;
                return true;
            }
            case "m_edgePath":
            case "edgePath":
            {
                if (value is null)
                {
                    instance.m_edgePath = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiEdgePath castValue)
                {
                    instance.m_edgePath = castValue;
                    return true;
                }
                return false;
            }
            case "m_stalenessChecker":
            case "stalenessChecker":
            {
                if (value is null)
                {
                    instance.m_stalenessChecker = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavigatorStalenessChecker castValue)
                {
                    instance.m_stalenessChecker = castValue;
                    return true;
                }
                return false;
            }
            case "m_traversalState":
            case "traversalState":
            {
                if (value is not Havok.hk2018.hkaiEdgePath.TraversalState castValue) return false;
                instance.m_traversalState = castValue;
                return true;
            }
            case "m_worldIndex":
            case "worldIndex":
            {
                if (value is not int castValue) return false;
                instance.m_worldIndex = castValue;
                return true;
            }
            case "m_pathRequestQueueId":
            case "pathRequestQueueId":
            {
                if (value is not Havok.hk2018.hkHandle<byte> castValue) return false;
                instance.m_pathRequestQueueId = castValue;
                return true;
            }
            case "m_acceptablePathQualities":
            case "acceptablePathQualities":
            {
                if (value is Havok.hk2018.hkaiNavigator.PathQualityBits castValue)
                {
                    instance.m_acceptablePathQualities = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_acceptablePathQualities = (Havok.hk2018.hkaiNavigator.PathQualityBits)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
