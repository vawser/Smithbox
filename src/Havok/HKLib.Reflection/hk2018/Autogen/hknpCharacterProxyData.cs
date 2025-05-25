// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpCharacterProxyData : HavokData<hknpCharacterProxy> 
{
    public hknpCharacterProxyData(HavokType type, hknpCharacterProxy instance) : base(type, instance) {}

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
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                if (instance.m_bodyId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
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
            case "m_lastDisplacement":
            case "lastDisplacement":
            {
                if (instance.m_lastDisplacement is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastVelocity":
            case "lastVelocity":
            {
                if (instance.m_lastVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastInvDeltaTime":
            case "lastInvDeltaTime":
            {
                if (instance.m_lastInvDeltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSlopeCosine":
            case "maxSlopeCosine":
            {
                if (instance.m_maxSlopeCosine is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicFriction":
            case "dynamicFriction":
            {
                if (instance.m_dynamicFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticFriction":
            case "staticFriction":
            {
                if (instance.m_staticFriction is not TGet castValue) return false;
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
            case "m_keepDistance":
            case "keepDistance":
            {
                if (instance.m_keepDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keepContactTolerance":
            case "keepContactTolerance":
            {
                if (instance.m_keepContactTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactAngleSensitivity":
            case "contactAngleSensitivity":
            {
                if (instance.m_contactAngleSensitivity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userPlanes":
            case "userPlanes":
            {
                if (instance.m_userPlanes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCharacterSpeedForSolver":
            case "maxCharacterSpeedForSolver":
            {
                if (instance.m_maxCharacterSpeedForSolver is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterStrength":
            case "characterStrength":
            {
                if (instance.m_characterStrength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterMass":
            case "characterMass":
            {
                if (instance.m_characterMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_penetrationRecoverySpeed":
            case "penetrationRecoverySpeed":
            {
                if (instance.m_penetrationRecoverySpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCastIterations":
            case "maxCastIterations":
            {
                if (instance.m_maxCastIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_refreshManifoldInCheckSupport":
            case "refreshManifoldInCheckSupport":
            {
                if (instance.m_refreshManifoldInCheckSupport is not TGet castValue) return false;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyId = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_velocity = castValue;
                return true;
            }
            case "m_lastDisplacement":
            case "lastDisplacement":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lastDisplacement = castValue;
                return true;
            }
            case "m_lastVelocity":
            case "lastVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lastVelocity = castValue;
                return true;
            }
            case "m_lastInvDeltaTime":
            case "lastInvDeltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_lastInvDeltaTime = castValue;
                return true;
            }
            case "m_maxSlopeCosine":
            case "maxSlopeCosine":
            {
                if (value is not float castValue) return false;
                instance.m_maxSlopeCosine = castValue;
                return true;
            }
            case "m_dynamicFriction":
            case "dynamicFriction":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicFriction = castValue;
                return true;
            }
            case "m_staticFriction":
            case "staticFriction":
            {
                if (value is not float castValue) return false;
                instance.m_staticFriction = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_keepDistance":
            case "keepDistance":
            {
                if (value is not float castValue) return false;
                instance.m_keepDistance = castValue;
                return true;
            }
            case "m_keepContactTolerance":
            case "keepContactTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_keepContactTolerance = castValue;
                return true;
            }
            case "m_contactAngleSensitivity":
            case "contactAngleSensitivity":
            {
                if (value is not float castValue) return false;
                instance.m_contactAngleSensitivity = castValue;
                return true;
            }
            case "m_userPlanes":
            case "userPlanes":
            {
                if (value is not int castValue) return false;
                instance.m_userPlanes = castValue;
                return true;
            }
            case "m_maxCharacterSpeedForSolver":
            case "maxCharacterSpeedForSolver":
            {
                if (value is not float castValue) return false;
                instance.m_maxCharacterSpeedForSolver = castValue;
                return true;
            }
            case "m_characterStrength":
            case "characterStrength":
            {
                if (value is not float castValue) return false;
                instance.m_characterStrength = castValue;
                return true;
            }
            case "m_characterMass":
            case "characterMass":
            {
                if (value is not float castValue) return false;
                instance.m_characterMass = castValue;
                return true;
            }
            case "m_penetrationRecoverySpeed":
            case "penetrationRecoverySpeed":
            {
                if (value is not float castValue) return false;
                instance.m_penetrationRecoverySpeed = castValue;
                return true;
            }
            case "m_maxCastIterations":
            case "maxCastIterations":
            {
                if (value is not int castValue) return false;
                instance.m_maxCastIterations = castValue;
                return true;
            }
            case "m_refreshManifoldInCheckSupport":
            case "refreshManifoldInCheckSupport":
            {
                if (value is not bool castValue) return false;
                instance.m_refreshManifoldInCheckSupport = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
