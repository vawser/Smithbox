// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpCharacterProxyCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpCharacterProxyCinfo>
{
    public hkpCharacterProxyCinfoData(Havok.hk2014.hkpCharacterProxyCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_keepContactTolerance":
            case "keepContactTolerance":
            {
                value = instance.m_keepContactTolerance;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_extraUpStaticFriction":
            case "extraUpStaticFriction":
            {
                value = instance.m_extraUpStaticFriction;
                return true;
            }
            case "m_extraDownStaticFriction":
            case "extraDownStaticFriction":
            {
                value = instance.m_extraDownStaticFriction;
                return true;
            }
            case "m_shapePhantom":
            case "shapePhantom":
            {
                value = instance.m_shapePhantom;
                return true;
            }
            case "m_keepDistance":
            case "keepDistance":
            {
                value = instance.m_keepDistance;
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
            case "m_maxSlope":
            case "maxSlope":
            {
                value = instance.m_maxSlope;
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
            case "m_keepContactTolerance":
            case "keepContactTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_keepContactTolerance = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_extraUpStaticFriction":
            case "extraUpStaticFriction":
            {
                if (value is not float castValue) return false;
                instance.m_extraUpStaticFriction = castValue;
                return true;
            }
            case "m_extraDownStaticFriction":
            case "extraDownStaticFriction":
            {
                if (value is not float castValue) return false;
                instance.m_extraDownStaticFriction = castValue;
                return true;
            }
            case "m_shapePhantom":
            case "shapePhantom":
            {
                if (value is null)
                {
                    instance.m_shapePhantom = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpShapePhantom castValue)
                {
                    instance.m_shapePhantom = castValue;
                    return true;
                }
                return false;
            }
            case "m_keepDistance":
            case "keepDistance":
            {
                if (value is not float castValue) return false;
                instance.m_keepDistance = castValue;
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
                if (value is not uint castValue) return false;
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
            case "m_maxSlope":
            case "maxSlope":
            {
                if (value is not float castValue) return false;
                instance.m_maxSlope = castValue;
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
