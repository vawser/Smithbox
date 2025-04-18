// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiCollisionAvoidance;

namespace HKLib.Reflection.hk2018;

internal class hkaiCollisionAvoidanceCharacterData : HavokData<Character> 
{
    public hkaiCollisionAvoidanceCharacterData(HavokType type, Character instance) : base(type, instance) {}

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
            case "m_boundaryGathererData":
            case "boundaryGathererData":
            {
                if (instance.m_boundaryGathererData is not TGet castValue) return false;
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
            case "m_velocity":
            case "velocity":
            {
                if (instance.m_velocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (instance.m_surfaceVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distanceToLocalGoal":
            case "distanceToLocalGoal":
            {
                if (instance.m_distanceToLocalGoal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localGoalPlane":
            case "localGoalPlane":
            {
                if (instance.m_localGoalPlane is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredDirection":
            case "desiredDirection":
            {
                if (instance.m_desiredDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avoidanceVelocity":
            case "avoidanceVelocity":
            {
                if (instance.m_avoidanceVelocity is not TGet castValue) return false;
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
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                if (instance.m_maximumSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_preferredSpeed":
            case "preferredSpeed":
            {
                if (instance.m_preferredSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sensorSize":
            case "sensorSize":
            {
                if (instance.m_sensorSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumAvoidanceCharacters":
            case "maximumAvoidanceCharacters":
            {
                if (instance.m_maximumAvoidanceCharacters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_steeringProperties":
            case "steeringProperties":
            {
                if (instance.m_steeringProperties is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundaryGatherer":
            case "boundaryGatherer":
            {
                if (instance.m_boundaryGatherer is null)
                {
                    return true;
                }
                if (instance.m_boundaryGatherer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_scoreModifiers":
            case "scoreModifiers":
            {
                if (instance.m_scoreModifiers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_steeringEnabled":
            case "steeringEnabled":
            {
                if (instance.m_steeringEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_system":
            case "system":
            {
                if (instance.m_system is null)
                {
                    return true;
                }
                if (instance.m_system is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexInSystem":
            case "indexInSystem":
            {
                if (instance.m_indexInSystem is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enabledIndexInSystem":
            case "enabledIndexInSystem":
            {
                if (instance.m_enabledIndexInSystem is not TGet castValue) return false;
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
            case "m_boundaryGathererData":
            case "boundaryGathererData":
            {
                if (value is not ulong castValue) return false;
                instance.m_boundaryGathererData = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_velocity = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_surfaceVelocity = castValue;
                return true;
            }
            case "m_distanceToLocalGoal":
            case "distanceToLocalGoal":
            {
                if (value is not float castValue) return false;
                instance.m_distanceToLocalGoal = castValue;
                return true;
            }
            case "m_localGoalPlane":
            case "localGoalPlane":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_localGoalPlane = castValue;
                return true;
            }
            case "m_desiredDirection":
            case "desiredDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_desiredDirection = castValue;
                return true;
            }
            case "m_avoidanceVelocity":
            case "avoidanceVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_avoidanceVelocity = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maximumSpeed = castValue;
                return true;
            }
            case "m_preferredSpeed":
            case "preferredSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_preferredSpeed = castValue;
                return true;
            }
            case "m_sensorSize":
            case "sensorSize":
            {
                if (value is not Character.SensorSize castValue) return false;
                instance.m_sensorSize = castValue;
                return true;
            }
            case "m_maximumAvoidanceCharacters":
            case "maximumAvoidanceCharacters":
            {
                if (value is not int castValue) return false;
                instance.m_maximumAvoidanceCharacters = castValue;
                return true;
            }
            case "m_steeringProperties":
            case "steeringProperties":
            {
                if (value is not SteeringProperties castValue) return false;
                instance.m_steeringProperties = castValue;
                return true;
            }
            case "m_boundaryGatherer":
            case "boundaryGatherer":
            {
                if (value is null)
                {
                    instance.m_boundaryGatherer = default;
                    return true;
                }
                if (value is BoundaryGatherer castValue)
                {
                    instance.m_boundaryGatherer = castValue;
                    return true;
                }
                return false;
            }
            case "m_scoreModifiers":
            case "scoreModifiers":
            {
                if (value is not List<ReferencedScoreModifier?> castValue) return false;
                instance.m_scoreModifiers = castValue;
                return true;
            }
            case "m_steeringEnabled":
            case "steeringEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_steeringEnabled = castValue;
                return true;
            }
            case "m_system":
            case "system":
            {
                if (value is null)
                {
                    instance.m_system = default;
                    return true;
                }
                if (value is HKLib.hk2018.hkaiCollisionAvoidance.System castValue)
                {
                    instance.m_system = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexInSystem":
            case "indexInSystem":
            {
                if (value is not int castValue) return false;
                instance.m_indexInSystem = castValue;
                return true;
            }
            case "m_enabledIndexInSystem":
            case "enabledIndexInSystem":
            {
                if (value is not int castValue) return false;
                instance.m_enabledIndexInSystem = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
