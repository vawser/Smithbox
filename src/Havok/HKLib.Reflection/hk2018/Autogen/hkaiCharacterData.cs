// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiCharacterData : HavokData<hkaiCharacter> 
{
    public hkaiCharacterData(HavokType type, hkaiCharacter instance) : base(type, instance) {}

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
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forward":
            case "forward":
            {
                if (instance.m_forward is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (instance.m_velocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentNavMeshFace":
            case "currentNavMeshFace":
            {
                if (instance.m_currentNavMeshFace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentNavVolumeCell":
            case "currentNavVolumeCell":
            {
                if (instance.m_currentNavVolumeCell is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (instance.m_radius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredSpeed":
            case "desiredSpeed":
            {
                if (instance.m_desiredSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_adaptiveRanger":
            case "adaptiveRanger":
            {
                if (instance.m_adaptiveRanger is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_costModifier":
            case "costModifier":
            {
                if (instance.m_costModifier is null)
                {
                    return true;
                }
                if (instance.m_costModifier is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (instance.m_edgeFilter is null)
                {
                    return true;
                }
                if (instance.m_edgeFilter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_agentFilterInfo":
            case "agentFilterInfo":
            {
                if (instance.m_agentFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avoidanceProperties":
            case "avoidanceProperties":
            {
                if (instance.m_avoidanceProperties is null)
                {
                    return true;
                }
                if (instance.m_avoidanceProperties is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_avoidanceState":
            case "avoidanceState":
            {
                if (instance.m_avoidanceState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_agentPriority":
            case "agentPriority":
            {
                if (instance.m_agentPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avoidanceType":
            case "avoidanceType":
            {
                if (instance.m_avoidanceType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avoidanceEnabledMask":
            case "avoidanceEnabledMask":
            {
                if (instance.m_avoidanceEnabledMask is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_avoidanceEnabledMask is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_state":
            case "state":
            {
                if (instance.m_state is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_state is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (instance.m_layerIndex is not TGet castValue) return false;
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_forward":
            case "forward":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_forward = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_velocity = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_currentNavMeshFace":
            case "currentNavMeshFace":
            {
                if (value is not uint castValue) return false;
                instance.m_currentNavMeshFace = castValue;
                return true;
            }
            case "m_currentNavVolumeCell":
            case "currentNavVolumeCell":
            {
                if (value is not uint castValue) return false;
                instance.m_currentNavVolumeCell = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_desiredSpeed":
            case "desiredSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_desiredSpeed = castValue;
                return true;
            }
            case "m_adaptiveRanger":
            case "adaptiveRanger":
            {
                if (value is not hkaiAdaptiveRanger castValue) return false;
                instance.m_adaptiveRanger = castValue;
                return true;
            }
            case "m_costModifier":
            case "costModifier":
            {
                if (value is null)
                {
                    instance.m_costModifier = default;
                    return true;
                }
                if (value is hkaiAstarCostModifier castValue)
                {
                    instance.m_costModifier = castValue;
                    return true;
                }
                return false;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (value is null)
                {
                    instance.m_edgeFilter = default;
                    return true;
                }
                if (value is hkaiAstarEdgeFilter castValue)
                {
                    instance.m_edgeFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_agentFilterInfo":
            case "agentFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_agentFilterInfo = castValue;
                return true;
            }
            case "m_avoidanceProperties":
            case "avoidanceProperties":
            {
                if (value is null)
                {
                    instance.m_avoidanceProperties = default;
                    return true;
                }
                if (value is hkaiAvoidanceProperties castValue)
                {
                    instance.m_avoidanceProperties = castValue;
                    return true;
                }
                return false;
            }
            case "m_avoidanceState":
            case "avoidanceState":
            {
                if (value is not float castValue) return false;
                instance.m_avoidanceState = castValue;
                return true;
            }
            case "m_agentPriority":
            case "agentPriority":
            {
                if (value is not uint castValue) return false;
                instance.m_agentPriority = castValue;
                return true;
            }
            case "m_avoidanceType":
            case "avoidanceType":
            {
                if (value is not ushort castValue) return false;
                instance.m_avoidanceType = castValue;
                return true;
            }
            case "m_avoidanceEnabledMask":
            case "avoidanceEnabledMask":
            {
                if (value is hkaiCharacter.AvoidanceEnabledMaskBits castValue)
                {
                    instance.m_avoidanceEnabledMask = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_avoidanceEnabledMask = (hkaiCharacter.AvoidanceEnabledMaskBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_state":
            case "state":
            {
                if (value is hkaiCharacter.State castValue)
                {
                    instance.m_state = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_state = (hkaiCharacter.State)intValue;
                    return true;
                }
                return false;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
