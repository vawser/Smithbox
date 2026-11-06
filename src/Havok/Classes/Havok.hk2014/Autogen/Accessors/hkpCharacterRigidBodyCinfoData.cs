// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpCharacterRigidBodyCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpCharacterRigidBodyCinfo>
{
    public hkpCharacterRigidBodyCinfoData(Havok.hk2014.hkpCharacterRigidBodyCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                value = instance.m_rotation;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                value = instance.m_friction;
                return true;
            }
            case "m_maxLinearVelocity":
            case "maxLinearVelocity":
            {
                value = instance.m_maxLinearVelocity;
                return true;
            }
            case "m_allowedPenetrationDepth":
            case "allowedPenetrationDepth":
            {
                value = instance.m_allowedPenetrationDepth;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_maxSlope":
            case "maxSlope":
            {
                value = instance.m_maxSlope;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                value = instance.m_maxForce;
                return true;
            }
            case "m_unweldingHeightOffsetFactor":
            case "unweldingHeightOffsetFactor":
            {
                value = instance.m_unweldingHeightOffsetFactor;
                return true;
            }
            case "m_maxSpeedForSimplexSolver":
            case "maxSpeedForSimplexSolver":
            {
                value = instance.m_maxSpeedForSimplexSolver;
                return true;
            }
            case "m_supportDistance":
            case "supportDistance":
            {
                value = instance.m_supportDistance;
                return true;
            }
            case "m_hardSupportDistance":
            case "hardSupportDistance":
            {
                value = instance.m_hardSupportDistance;
                return true;
            }
            case "m_vdbColor":
            case "vdbColor":
            {
                value = instance.m_vdbColor;
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
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
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
                if (value is Havok.hk2014.hkpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_rotation":
            case "rotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not float castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_maxLinearVelocity":
            case "maxLinearVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_maxLinearVelocity = castValue;
                return true;
            }
            case "m_allowedPenetrationDepth":
            case "allowedPenetrationDepth":
            {
                if (value is not float castValue) return false;
                instance.m_allowedPenetrationDepth = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_maxSlope":
            case "maxSlope":
            {
                if (value is not float castValue) return false;
                instance.m_maxSlope = castValue;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxForce = castValue;
                return true;
            }
            case "m_unweldingHeightOffsetFactor":
            case "unweldingHeightOffsetFactor":
            {
                if (value is not float castValue) return false;
                instance.m_unweldingHeightOffsetFactor = castValue;
                return true;
            }
            case "m_maxSpeedForSimplexSolver":
            case "maxSpeedForSimplexSolver":
            {
                if (value is not float castValue) return false;
                instance.m_maxSpeedForSimplexSolver = castValue;
                return true;
            }
            case "m_supportDistance":
            case "supportDistance":
            {
                if (value is not float castValue) return false;
                instance.m_supportDistance = castValue;
                return true;
            }
            case "m_hardSupportDistance":
            case "hardSupportDistance":
            {
                if (value is not float castValue) return false;
                instance.m_hardSupportDistance = castValue;
                return true;
            }
            case "m_vdbColor":
            case "vdbColor":
            {
                if (value is not int castValue) return false;
                instance.m_vdbColor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
