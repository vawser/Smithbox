// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpVelocityConstraintMotorData : HavokData<hkpVelocityConstraintMotor> 
{
    public hkpVelocityConstraintMotorData(HavokType type, hkpVelocityConstraintMotor instance) : base(type, instance) {}

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
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_minForce":
            case "minForce":
            {
                if (instance.m_minForce is not TGet castValue) return false;
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
            case "m_tau":
            case "tau":
            {
                if (instance.m_tau is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (instance.m_damping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityTarget":
            case "velocityTarget":
            {
                if (instance.m_velocityTarget is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useVelocityTargetFromConstraintTargets":
            case "useVelocityTargetFromConstraintTargets":
            {
                if (instance.m_useVelocityTargetFromConstraintTargets is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkpConstraintMotor.MotorType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_minForce":
            case "minForce":
            {
                if (value is not float castValue) return false;
                instance.m_minForce = castValue;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxForce = castValue;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                if (value is not float castValue) return false;
                instance.m_tau = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_velocityTarget":
            case "velocityTarget":
            {
                if (value is not float castValue) return false;
                instance.m_velocityTarget = castValue;
                return true;
            }
            case "m_useVelocityTargetFromConstraintTargets":
            case "useVelocityTargetFromConstraintTargets":
            {
                if (value is not bool castValue) return false;
                instance.m_useVelocityTargetFromConstraintTargets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
