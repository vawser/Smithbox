// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpLinFrictionConstraintAtomData : HavokData<hkpLinFrictionConstraintAtom> 
{
    public hkpLinFrictionConstraintAtomData(HavokType type, hkpLinFrictionConstraintAtom instance) : base(type, instance) {}

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
            case "m_frictionAxis":
            case "frictionAxis":
            {
                if (instance.m_frictionAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (instance.m_maxFrictionForce is not TGet castValue) return false;
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
            case "m_frictionAxis":
            case "frictionAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_frictionAxis = castValue;
                return true;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxFrictionForce = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
