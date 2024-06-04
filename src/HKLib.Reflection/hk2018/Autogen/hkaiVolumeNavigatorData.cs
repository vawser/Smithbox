// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiVolumeNavigatorData : HavokData<hkaiVolumeNavigator> 
{
    public hkaiVolumeNavigatorData(HavokType type, hkaiVolumeNavigator instance) : base(type, instance) {}

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
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentGoals":
            case "currentGoals":
            {
                if (instance.m_currentGoals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rawPosition":
            case "rawPosition":
            {
                if (instance.m_rawPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastIdealDirection":
            case "lastIdealDirection":
            {
                if (instance.m_lastIdealDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasLastIdealDirection":
            case "hasLastIdealDirection":
            {
                if (instance.m_hasLastIdealDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (instance.m_settings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasPosition":
            case "hasPosition":
            {
                if (instance.m_hasPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_world":
            case "world":
            {
                if (instance.m_world is null)
                {
                    return true;
                }
                if (instance.m_world is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_lastNavigationOutput":
            case "lastNavigationOutput":
            {
                if (instance.m_lastNavigationOutput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                if (instance.m_pathRequest is null)
                {
                    return true;
                }
                if (instance.m_pathRequest is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_goalReached":
            case "goalReached":
            {
                if (instance.m_goalReached is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gatePath":
            case "gatePath":
            {
                if (instance.m_gatePath is null)
                {
                    return true;
                }
                if (instance.m_gatePath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_stalenessChecker":
            case "stalenessChecker":
            {
                if (instance.m_stalenessChecker is null)
                {
                    return true;
                }
                if (instance.m_stalenessChecker is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_traversalState":
            case "traversalState":
            {
                if (instance.m_traversalState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_highestReportedUserEdgeEntry":
            case "highestReportedUserEdgeEntry":
            {
                if (instance.m_highestReportedUserEdgeEntry is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldIndex":
            case "worldIndex":
            {
                if (instance.m_worldIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathRequestQueueId":
            case "pathRequestQueueId":
            {
                if (instance.m_pathRequestQueueId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_acceptablePathQualities":
            case "acceptablePathQualities":
            {
                if (instance.m_acceptablePathQualities is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_acceptablePathQualities is TGet byteValue)
                {
                    value = byteValue;
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
                if (value is not List<hkaiVolumeNavigator.Goal> castValue) return false;
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
            case "m_lastIdealDirection":
            case "lastIdealDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lastIdealDirection = castValue;
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
                if (value is not hkaiVolumeNavigator.NavigatorSettings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            case "m_hasPosition":
            case "hasPosition":
            {
                if (value is not bool castValue) return false;
                instance.m_hasPosition = castValue;
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
                if (value is hkaiWorld castValue)
                {
                    instance.m_world = castValue;
                    return true;
                }
                return false;
            }
            case "m_lastNavigationOutput":
            case "lastNavigationOutput":
            {
                if (value is not hkaiVolumeNavigator.NavigationOutput castValue) return false;
                instance.m_lastNavigationOutput = castValue;
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
                if (value is hkaiVolumeNavigator.PathRequest castValue)
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
            case "m_gatePath":
            case "gatePath":
            {
                if (value is null)
                {
                    instance.m_gatePath = default;
                    return true;
                }
                if (value is hkaiGatePath castValue)
                {
                    instance.m_gatePath = castValue;
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
                if (value is hkaiVolumeNavigatorStalenessChecker castValue)
                {
                    instance.m_stalenessChecker = castValue;
                    return true;
                }
                return false;
            }
            case "m_traversalState":
            case "traversalState":
            {
                if (value is not hkaiGatePath.TraversalState castValue) return false;
                instance.m_traversalState = castValue;
                return true;
            }
            case "m_highestReportedUserEdgeEntry":
            case "highestReportedUserEdgeEntry":
            {
                if (value is not int castValue) return false;
                instance.m_highestReportedUserEdgeEntry = castValue;
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
                if (value is not hkHandle<byte> castValue) return false;
                instance.m_pathRequestQueueId = castValue;
                return true;
            }
            case "m_acceptablePathQualities":
            case "acceptablePathQualities":
            {
                if (value is hkaiVolumeNavigator.PathQualityBits castValue)
                {
                    instance.m_acceptablePathQualities = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_acceptablePathQualities = (hkaiVolumeNavigator.PathQualityBits)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
