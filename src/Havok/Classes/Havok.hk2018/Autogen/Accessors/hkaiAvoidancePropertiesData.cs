// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiAvoidancePropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiAvoidanceProperties>
{
    public hkaiAvoidancePropertiesData(Havok.hk2018.hkaiAvoidanceProperties instance) : base(instance) { }

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
            case "m_movementProperties":
            case "movementProperties":
            {
                value = instance.m_movementProperties;
                return true;
            }
            case "m_nearbyBoundariesSearchType":
            case "nearbyBoundariesSearchType":
            {
                value = instance.m_nearbyBoundariesSearchType;
                return true;
            }
            case "m_localSensorAabb":
            case "localSensorAabb":
            {
                value = instance.m_localSensorAabb;
                return true;
            }
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
            case "m_maxNeighbors":
            case "maxNeighbors":
            {
                value = instance.m_maxNeighbors;
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
            case "m_movementProperties":
            case "movementProperties":
            {
                if (value is not Havok.hk2018.hkaiMovementProperties castValue) return false;
                instance.m_movementProperties = castValue;
                return true;
            }
            case "m_nearbyBoundariesSearchType":
            case "nearbyBoundariesSearchType":
            {
                if (value is Havok.hk2018.hkaiAvoidanceProperties.NearbyBoundariesSearchType castValue)
                {
                    instance.m_nearbyBoundariesSearchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_nearbyBoundariesSearchType = (Havok.hk2018.hkaiAvoidanceProperties.NearbyBoundariesSearchType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_localSensorAabb":
            case "localSensorAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_localSensorAabb = castValue;
                return true;
            }
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
            case "m_maxNeighbors":
            case "maxNeighbors":
            {
                if (value is not int castValue) return false;
                instance.m_maxNeighbors = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
