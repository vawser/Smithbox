// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCollisionAvoidanceSteeringPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCollisionAvoidance.SteeringProperties>
{
    public hkaiCollisionAvoidanceSteeringPropertiesData(Havok.hk2019.hkaiCollisionAvoidance.SteeringProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_wallFollowingAngle":
            case "wallFollowingAngle":
            {
                value = instance.m_wallFollowingAngle;
                return true;
            }
            case "m_dodgingPenalty":
            case "dodgingPenalty":
            {
                value = instance.m_dodgingPenalty;
                return true;
            }
            case "m_velocityHysteresis":
            case "velocityHysteresis":
            {
                value = instance.m_velocityHysteresis;
                return true;
            }
            case "m_sidednessChangingPenalty":
            case "sidednessChangingPenalty":
            {
                value = instance.m_sidednessChangingPenalty;
                return true;
            }
            case "m_collisionPenalty":
            case "collisionPenalty":
            {
                value = instance.m_collisionPenalty;
                return true;
            }
            case "m_penetrationPenalty":
            case "penetrationPenalty":
            {
                value = instance.m_penetrationPenalty;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
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
            case "m_wallFollowingAngle":
            case "wallFollowingAngle":
            {
                if (value is not float castValue) return false;
                instance.m_wallFollowingAngle = castValue;
                return true;
            }
            case "m_dodgingPenalty":
            case "dodgingPenalty":
            {
                if (value is not float castValue) return false;
                instance.m_dodgingPenalty = castValue;
                return true;
            }
            case "m_velocityHysteresis":
            case "velocityHysteresis":
            {
                if (value is not float castValue) return false;
                instance.m_velocityHysteresis = castValue;
                return true;
            }
            case "m_sidednessChangingPenalty":
            case "sidednessChangingPenalty":
            {
                if (value is not float castValue) return false;
                instance.m_sidednessChangingPenalty = castValue;
                return true;
            }
            case "m_collisionPenalty":
            case "collisionPenalty":
            {
                if (value is not float castValue) return false;
                instance.m_collisionPenalty = castValue;
                return true;
            }
            case "m_penetrationPenalty":
            case "penetrationPenalty":
            {
                if (value is not float castValue) return false;
                instance.m_penetrationPenalty = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                if (value is not int castValue) return false;
                instance.m_priority = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
