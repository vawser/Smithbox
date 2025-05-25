// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpPrismaticConstraintDataAtomsData : HavokData<hkpPrismaticConstraintData.Atoms> 
{
    public hkpPrismaticConstraintDataAtomsData(HavokType type, hkpPrismaticConstraintData.Atoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transforms":
            case "transforms":
            {
                if (instance.m_transforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                if (instance.m_motor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (instance.m_friction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (instance.m_ang is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lin0":
            case "lin0":
            {
                if (instance.m_lin0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lin1":
            case "lin1":
            {
                if (instance.m_lin1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linLimit":
            case "linLimit":
            {
                if (instance.m_linLimit is not TGet castValue) return false;
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
            case "m_transforms":
            case "transforms":
            {
                if (value is not hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                if (value is not hkpLinMotorConstraintAtom castValue) return false;
                instance.m_motor = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not hkpAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            case "m_lin0":
            case "lin0":
            {
                if (value is not hkpLinConstraintAtom castValue) return false;
                instance.m_lin0 = castValue;
                return true;
            }
            case "m_lin1":
            case "lin1":
            {
                if (value is not hkpLinConstraintAtom castValue) return false;
                instance.m_lin1 = castValue;
                return true;
            }
            case "m_linLimit":
            case "linLimit":
            {
                if (value is not hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
