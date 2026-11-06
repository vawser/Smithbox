// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiCharacterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiCharacter>
{
    public hkaiCharacterData(Havok.hk2014.hkaiCharacter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_forward":
            case "forward":
            {
                value = instance.m_forward;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                value = instance.m_velocity;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_currentNavMeshFace":
            case "currentNavMeshFace":
            {
                value = instance.m_currentNavMeshFace;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_desiredSpeed":
            case "desiredSpeed":
            {
                value = instance.m_desiredSpeed;
                return true;
            }
            case "m_adaptiveRanger":
            case "adaptiveRanger":
            {
                value = instance.m_adaptiveRanger;
                return true;
            }
            case "m_costModifier":
            case "costModifier":
            {
                value = instance.m_costModifier;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                value = instance.m_edgeFilter;
                return true;
            }
            case "m_agentFilterInfo":
            case "agentFilterInfo":
            {
                value = instance.m_agentFilterInfo;
                return true;
            }
            case "m_avoidanceProperties":
            case "avoidanceProperties":
            {
                value = instance.m_avoidanceProperties;
                return true;
            }
            case "m_avoidanceState":
            case "avoidanceState":
            {
                value = instance.m_avoidanceState;
                return true;
            }
            case "m_agentPriority":
            case "agentPriority":
            {
                value = instance.m_agentPriority;
                return true;
            }
            case "m_avoidanceType":
            case "avoidanceType":
            {
                value = instance.m_avoidanceType;
                return true;
            }
            case "m_avoidanceEnabledMask":
            case "avoidanceEnabledMask":
            {
                value = instance.m_avoidanceEnabledMask;
                return true;
            }
            case "m_state":
            case "state":
            {
                value = instance.m_state;
                return true;
            }
            case "m_layer":
            case "layer":
            {
                value = instance.m_layer;
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
                if (value is not Havok.hk2014.hkaiAdaptiveRanger castValue) return false;
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
                if (value is Havok.hk2014.hkaiAstarCostModifier castValue)
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
                if (value is Havok.hk2014.hkaiAstarEdgeFilter castValue)
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
                if (value is Havok.hk2014.hkaiAvoidanceProperties castValue)
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
                if (value is Havok.hk2014.hkaiCharacter.AvoidanceEnabledMaskBits castValue)
                {
                    instance.m_avoidanceEnabledMask = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_avoidanceEnabledMask = (Havok.hk2014.hkaiCharacter.AvoidanceEnabledMaskBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_state":
            case "state":
            {
                if (value is Havok.hk2014.hkaiCharacter.State castValue)
                {
                    instance.m_state = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_state = (Havok.hk2014.hkaiCharacter.State)intValue;
                    return true;
                }
                return false;
            }
            case "m_layer":
            case "layer":
            {
                if (value is not uint castValue) return false;
                instance.m_layer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
