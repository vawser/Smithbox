// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAvoidancePropertiesData : HavokData<hkaiAvoidanceProperties> 
{
    public hkaiAvoidancePropertiesData(HavokType type, hkaiAvoidanceProperties instance) : base(type, instance) {}

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
            case "m_movementProperties":
            case "movementProperties":
            {
                if (instance.m_movementProperties is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nearbyBoundariesSearchType":
            case "nearbyBoundariesSearchType":
            {
                if (instance.m_nearbyBoundariesSearchType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_nearbyBoundariesSearchType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_localSensorAabb":
            case "localSensorAabb":
            {
                if (instance.m_localSensorAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_maxNeighbors":
            case "maxNeighbors":
            {
                if (instance.m_maxNeighbors is not TGet castValue) return false;
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
            case "m_movementProperties":
            case "movementProperties":
            {
                if (value is not hkaiMovementProperties castValue) return false;
                instance.m_movementProperties = castValue;
                return true;
            }
            case "m_nearbyBoundariesSearchType":
            case "nearbyBoundariesSearchType":
            {
                if (value is hkaiAvoidanceProperties.NearbyBoundariesSearchType castValue)
                {
                    instance.m_nearbyBoundariesSearchType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_nearbyBoundariesSearchType = (hkaiAvoidanceProperties.NearbyBoundariesSearchType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_localSensorAabb":
            case "localSensorAabb":
            {
                if (value is not hkAabb castValue) return false;
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
