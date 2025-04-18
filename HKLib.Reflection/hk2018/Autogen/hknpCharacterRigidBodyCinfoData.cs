// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpCharacterRigidBodyCinfoData : HavokData<hknpCharacterRigidBodyCinfo> 
{
    public hknpCharacterRigidBodyCinfoData(HavokType type, hknpCharacterRigidBodyCinfo instance) : base(type, instance) {}

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
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
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
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                if (instance.m_orientation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (instance.m_mass is not TGet castValue) return false;
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
            case "m_weldingTolerance":
            case "weldingTolerance":
            {
                if (instance.m_weldingTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredBodyId":
            case "desiredBodyId":
            {
                if (instance.m_desiredBodyId is not TGet castValue) return false;
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
            case "m_maxSlope":
            case "maxSlope":
            {
                if (instance.m_maxSlope is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSupportSlope":
            case "maxSupportSlope":
            {
                if (instance.m_maxSupportSlope is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                if (instance.m_maxForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSpeedForSimplexSolver":
            case "maxSpeedForSimplexSolver":
            {
                if (instance.m_maxSpeedForSimplexSolver is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_supportDistance":
            case "supportDistance":
            {
                if (instance.m_supportDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hardSupportDistance":
            case "hardSupportDistance":
            {
                if (instance.m_hardSupportDistance is not TGet castValue) return false;
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
                if (value is hknpShape castValue)
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
                if (value is not hknpBodyId castValue) return false;
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
            default:
            return false;
        }
    }

}
