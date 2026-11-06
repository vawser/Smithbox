// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpCharacterProxyData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpCharacterProxy>
{
    public hknpCharacterProxyData(Havok.hk2018.hknpCharacterProxy instance) : base(instance) { }

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
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_bodyId":
            case "bodyId":
            {
                value = instance.m_bodyId;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                value = instance.m_velocity;
                return true;
            }
            case "m_lastDisplacement":
            case "lastDisplacement":
            {
                value = instance.m_lastDisplacement;
                return true;
            }
            case "m_lastVelocity":
            case "lastVelocity":
            {
                value = instance.m_lastVelocity;
                return true;
            }
            case "m_lastInvDeltaTime":
            case "lastInvDeltaTime":
            {
                value = instance.m_lastInvDeltaTime;
                return true;
            }
            case "m_maxSlopeCosine":
            case "maxSlopeCosine":
            {
                value = instance.m_maxSlopeCosine;
                return true;
            }
            case "m_dynamicFriction":
            case "dynamicFriction":
            {
                value = instance.m_dynamicFriction;
                return true;
            }
            case "m_staticFriction":
            case "staticFriction":
            {
                value = instance.m_staticFriction;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_keepDistance":
            case "keepDistance":
            {
                value = instance.m_keepDistance;
                return true;
            }
            case "m_keepContactTolerance":
            case "keepContactTolerance":
            {
                value = instance.m_keepContactTolerance;
                return true;
            }
            case "m_contactAngleSensitivity":
            case "contactAngleSensitivity":
            {
                value = instance.m_contactAngleSensitivity;
                return true;
            }
            case "m_userPlanes":
            case "userPlanes":
            {
                value = instance.m_userPlanes;
                return true;
            }
            case "m_maxCharacterSpeedForSolver":
            case "maxCharacterSpeedForSolver":
            {
                value = instance.m_maxCharacterSpeedForSolver;
                return true;
            }
            case "m_characterStrength":
            case "characterStrength":
            {
                value = instance.m_characterStrength;
                return true;
            }
            case "m_characterMass":
            case "characterMass":
            {
                value = instance.m_characterMass;
                return true;
            }
            case "m_penetrationRecoverySpeed":
            case "penetrationRecoverySpeed":
            {
                value = instance.m_penetrationRecoverySpeed;
                return true;
            }
            case "m_maxCastIterations":
            case "maxCastIterations":
            {
                value = instance.m_maxCastIterations;
                return true;
            }
            case "m_refreshManifoldInCheckSupport":
            case "refreshManifoldInCheckSupport":
            {
                value = instance.m_refreshManifoldInCheckSupport;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpShape castValue)
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
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
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
                if (value is not Havok.hk2018.hkAabb castValue) return false;
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
