// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCollisionAvoidanceCharacterData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCollisionAvoidance.Character>
{
    public hkaiCollisionAvoidanceCharacterData(Havok.hk2018.hkaiCollisionAvoidance.Character instance) : base(instance) { }

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
            case "m_boundaryGathererData":
            case "boundaryGathererData":
            {
                value = instance.m_boundaryGathererData;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                value = instance.m_velocity;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                value = instance.m_surfaceVelocity;
                return true;
            }
            case "m_distanceToLocalGoal":
            case "distanceToLocalGoal":
            {
                value = instance.m_distanceToLocalGoal;
                return true;
            }
            case "m_localGoalPlane":
            case "localGoalPlane":
            {
                value = instance.m_localGoalPlane;
                return true;
            }
            case "m_desiredDirection":
            case "desiredDirection":
            {
                value = instance.m_desiredDirection;
                return true;
            }
            case "m_avoidanceVelocity":
            case "avoidanceVelocity":
            {
                value = instance.m_avoidanceVelocity;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                value = instance.m_maximumSpeed;
                return true;
            }
            case "m_preferredSpeed":
            case "preferredSpeed":
            {
                value = instance.m_preferredSpeed;
                return true;
            }
            case "m_sensorSize":
            case "sensorSize":
            {
                value = instance.m_sensorSize;
                return true;
            }
            case "m_maximumAvoidanceCharacters":
            case "maximumAvoidanceCharacters":
            {
                value = instance.m_maximumAvoidanceCharacters;
                return true;
            }
            case "m_steeringProperties":
            case "steeringProperties":
            {
                value = instance.m_steeringProperties;
                return true;
            }
            case "m_boundaryGatherer":
            case "boundaryGatherer":
            {
                value = instance.m_boundaryGatherer;
                return true;
            }
            case "m_scoreModifiers":
            case "scoreModifiers":
            {
                value = instance.m_scoreModifiers;
                return true;
            }
            case "m_steeringEnabled":
            case "steeringEnabled":
            {
                value = instance.m_steeringEnabled;
                return true;
            }
            case "m_system":
            case "system":
            {
                value = instance.m_system;
                return true;
            }
            case "m_indexInSystem":
            case "indexInSystem":
            {
                value = instance.m_indexInSystem;
                return true;
            }
            case "m_enabledIndexInSystem":
            case "enabledIndexInSystem":
            {
                value = instance.m_enabledIndexInSystem;
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
                if (value is not Havok.hk2018.hkaiCollisionAvoidance.Character.SensorSize castValue) return false;
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
                if (value is not Havok.hk2018.hkaiCollisionAvoidance.SteeringProperties castValue) return false;
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
                if (value is Havok.hk2018.hkaiCollisionAvoidance.BoundaryGatherer castValue)
                {
                    instance.m_boundaryGatherer = castValue;
                    return true;
                }
                return false;
            }
            case "m_scoreModifiers":
            case "scoreModifiers":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.ReferencedScoreModifier?> castValue) return false;
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
                if (value is Havok.hk2018.hkaiCollisionAvoidance.System castValue)
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
