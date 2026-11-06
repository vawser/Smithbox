// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpWheelFrictionConstraintAtomAxleData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpWheelFrictionConstraintAtom.Axle>
{
    public hkpWheelFrictionConstraintAtomAxleData(Havok.hk2014.hkpWheelFrictionConstraintAtom.Axle instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_spinVelocity":
            case "spinVelocity":
            {
                value = instance.m_spinVelocity;
                return true;
            }
            case "m_sumVelocity":
            case "sumVelocity":
            {
                value = instance.m_sumVelocity;
                return true;
            }
            case "m_numWheels":
            case "numWheels":
            {
                value = instance.m_numWheels;
                return true;
            }
            case "m_wheelsSolved":
            case "wheelsSolved":
            {
                value = instance.m_wheelsSolved;
                return true;
            }
            case "m_stepsSolved":
            case "stepsSolved":
            {
                value = instance.m_stepsSolved;
                return true;
            }
            case "m_invInertia":
            case "invInertia":
            {
                value = instance.m_invInertia;
                return true;
            }
            case "m_inertia":
            case "inertia":
            {
                value = instance.m_inertia;
                return true;
            }
            case "m_impulseScaling":
            case "impulseScaling":
            {
                value = instance.m_impulseScaling;
                return true;
            }
            case "m_impulseMax":
            case "impulseMax":
            {
                value = instance.m_impulseMax;
                return true;
            }
            case "m_isFixed":
            case "isFixed":
            {
                value = instance.m_isFixed;
                return true;
            }
            case "m_numWheelsOnGround":
            case "numWheelsOnGround":
            {
                value = instance.m_numWheelsOnGround;
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
