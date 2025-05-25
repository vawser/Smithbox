// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpWheelFrictionConstraintAtomAxleData : HavokData<hkpWheelFrictionConstraintAtom.Axle> 
{
    public hkpWheelFrictionConstraintAtomAxleData(HavokType type, hkpWheelFrictionConstraintAtom.Axle instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_spinVelocity":
            case "spinVelocity":
            {
                if (instance.m_spinVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sumVelocity":
            case "sumVelocity":
            {
                if (instance.m_sumVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numWheels":
            case "numWheels":
            {
                if (instance.m_numWheels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelsSolved":
            case "wheelsSolved":
            {
                if (instance.m_wheelsSolved is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stepsSolved":
            case "stepsSolved":
            {
                if (instance.m_stepsSolved is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invInertia":
            case "invInertia":
            {
                if (instance.m_invInertia is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inertia":
            case "inertia":
            {
                if (instance.m_inertia is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_impulseScaling":
            case "impulseScaling":
            {
                if (instance.m_impulseScaling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_impulseMax":
            case "impulseMax":
            {
                if (instance.m_impulseMax is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isFixed":
            case "isFixed":
            {
                if (instance.m_isFixed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numWheelsOnGround":
            case "numWheelsOnGround":
            {
                if (instance.m_numWheelsOnGround is not TGet castValue) return false;
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
            case "m_spinVelocity":
            case "spinVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_spinVelocity = castValue;
                return true;
            }
            case "m_sumVelocity":
            case "sumVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_sumVelocity = castValue;
                return true;
            }
            case "m_numWheels":
            case "numWheels":
            {
                if (value is not int castValue) return false;
                instance.m_numWheels = castValue;
                return true;
            }
            case "m_wheelsSolved":
            case "wheelsSolved":
            {
                if (value is not int castValue) return false;
                instance.m_wheelsSolved = castValue;
                return true;
            }
            case "m_stepsSolved":
            case "stepsSolved":
            {
                if (value is not int castValue) return false;
                instance.m_stepsSolved = castValue;
                return true;
            }
            case "m_invInertia":
            case "invInertia":
            {
                if (value is not float castValue) return false;
                instance.m_invInertia = castValue;
                return true;
            }
            case "m_inertia":
            case "inertia":
            {
                if (value is not float castValue) return false;
                instance.m_inertia = castValue;
                return true;
            }
            case "m_impulseScaling":
            case "impulseScaling":
            {
                if (value is not float castValue) return false;
                instance.m_impulseScaling = castValue;
                return true;
            }
            case "m_impulseMax":
            case "impulseMax":
            {
                if (value is not float castValue) return false;
                instance.m_impulseMax = castValue;
                return true;
            }
            case "m_isFixed":
            case "isFixed":
            {
                if (value is not bool castValue) return false;
                instance.m_isFixed = castValue;
                return true;
            }
            case "m_numWheelsOnGround":
            case "numWheelsOnGround":
            {
                if (value is not int castValue) return false;
                instance.m_numWheelsOnGround = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
