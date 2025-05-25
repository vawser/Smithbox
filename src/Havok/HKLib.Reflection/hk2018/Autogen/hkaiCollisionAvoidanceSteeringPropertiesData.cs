// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiCollisionAvoidance;

namespace HKLib.Reflection.hk2018;

internal class hkaiCollisionAvoidanceSteeringPropertiesData : HavokData<SteeringProperties> 
{
    public hkaiCollisionAvoidanceSteeringPropertiesData(HavokType type, SteeringProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_wallFollowingAngle":
            case "wallFollowingAngle":
            {
                if (instance.m_wallFollowingAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dodgingPenalty":
            case "dodgingPenalty":
            {
                if (instance.m_dodgingPenalty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityHysteresis":
            case "velocityHysteresis":
            {
                if (instance.m_velocityHysteresis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sidednessChangingPenalty":
            case "sidednessChangingPenalty":
            {
                if (instance.m_sidednessChangingPenalty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionPenalty":
            case "collisionPenalty":
            {
                if (instance.m_collisionPenalty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_penetrationPenalty":
            case "penetrationPenalty":
            {
                if (instance.m_penetrationPenalty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                if (instance.m_priority is not TGet castValue) return false;
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
