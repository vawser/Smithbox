// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpWheelFrictionConstraintAtomData : HavokData<hkpWheelFrictionConstraintAtom> 
{
    private static readonly System.Reflection.FieldInfo _frictionImpulseInfo = typeof(hkpWheelFrictionConstraintAtom).GetField("m_frictionImpulse")!;
    private static readonly System.Reflection.FieldInfo _slipImpulseInfo = typeof(hkpWheelFrictionConstraintAtom).GetField("m_slipImpulse")!;
    public hkpWheelFrictionConstraintAtomData(HavokType type, hkpWheelFrictionConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (instance.m_isEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forwardAxis":
            case "forwardAxis":
            {
                if (instance.m_forwardAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sideAxis":
            case "sideAxis":
            {
                if (instance.m_sideAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (instance.m_radius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                if (instance.m_axle is null)
                {
                    return true;
                }
                if (instance.m_axle is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (instance.m_maxFrictionForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_torque":
            case "torque":
            {
                if (instance.m_torque is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frictionImpulse":
            case "frictionImpulse":
            {
                if (instance.m_frictionImpulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_slipImpulse":
            case "slipImpulse":
            {
                if (instance.m_slipImpulse is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not byte castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_forwardAxis":
            case "forwardAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_forwardAxis = castValue;
                return true;
            }
            case "m_sideAxis":
            case "sideAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_sideAxis = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                if (value is null)
                {
                    instance.m_axle = default;
                    return true;
                }
                if (value is hkpWheelFrictionConstraintAtom.Axle castValue)
                {
                    instance.m_axle = castValue;
                    return true;
                }
                return false;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxFrictionForce = castValue;
                return true;
            }
            case "m_torque":
            case "torque":
            {
                if (value is not float castValue) return false;
                instance.m_torque = castValue;
                return true;
            }
            case "m_frictionImpulse":
            case "frictionImpulse":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _frictionImpulseInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_slipImpulse":
            case "slipImpulse":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _slipImpulseInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
