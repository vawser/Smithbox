// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpCharacterRigidBodyCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpCharacterRigidBodyCinfo>
{
    public hknpCharacterRigidBodyCinfoData(Havok.hk2019.hknpCharacterRigidBodyCinfo instance) : base(instance) { }

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
            case "m_orientation":
            case "orientation":
            {
                value = instance.m_orientation;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
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
            case "m_weldingTolerance":
            case "weldingTolerance":
            {
                value = instance.m_weldingTolerance;
                return true;
            }
            case "m_desiredBodyId":
            case "desiredBodyId":
            {
                value = instance.m_desiredBodyId;
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
            case "m_maxSupportSlope":
            case "maxSupportSlope":
            {
                value = instance.m_maxSupportSlope;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                value = instance.m_maxForce;
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
            case "m_massDistribution":
            case "massDistribution":
            {
                value = instance.m_massDistribution;
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
                if (value is Havok.hk2019.hknpShape castValue)
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
            case "m_orientation":
            case "orientation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_orientation = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
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
            case "m_weldingTolerance":
            case "weldingTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_weldingTolerance = castValue;
                return true;
            }
            case "m_desiredBodyId":
            case "desiredBodyId":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_desiredBodyId = castValue;
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
            case "m_maxSupportSlope":
            case "maxSupportSlope":
            {
                if (value is not float castValue) return false;
                instance.m_maxSupportSlope = castValue;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxForce = castValue;
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
            case "m_massDistribution":
            case "massDistribution":
            {
                if (value is null)
                {
                    instance.m_massDistribution = default;
                    return true;
                }
                if (value is Havok.hk2019.hknpRefMassDistribution castValue)
                {
                    instance.m_massDistribution = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
