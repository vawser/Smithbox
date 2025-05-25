// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpSetLocalRotationsConstraintAtomData : HavokData<hkpSetLocalRotationsConstraintAtom> 
{
    public hkpSetLocalRotationsConstraintAtomData(HavokType type, hkpSetLocalRotationsConstraintAtom instance) : base(type, instance) {}

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
            case "m_rotationA":
            case "rotationA":
            {
                if (instance.m_rotationA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationB":
            case "rotationB":
            {
                if (instance.m_rotationB is not TGet castValue) return false;
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
            case "m_rotationA":
            case "rotationA":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_rotationA = castValue;
                return true;
            }
            case "m_rotationB":
            case "rotationB":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_rotationB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
