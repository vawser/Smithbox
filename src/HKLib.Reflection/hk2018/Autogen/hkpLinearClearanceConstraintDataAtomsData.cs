// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpLinearClearanceConstraintDataAtomsData : HavokData<hkpLinearClearanceConstraintData.Atoms> 
{
    public hkpLinearClearanceConstraintDataAtomsData(HavokType type, hkpLinearClearanceConstraintData.Atoms instance) : base(type, instance) {}

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
            case "m_friction0":
            case "friction0":
            {
                if (instance.m_friction0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_friction1":
            case "friction1":
            {
                if (instance.m_friction1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_friction2":
            case "friction2":
            {
                if (instance.m_friction2 is not TGet castValue) return false;
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
            case "m_linLimit0":
            case "linLimit0":
            {
                if (instance.m_linLimit0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linLimit1":
            case "linLimit1":
            {
                if (instance.m_linLimit1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linLimit2":
            case "linLimit2":
            {
                if (instance.m_linLimit2 is not TGet castValue) return false;
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
            case "m_friction0":
            case "friction0":
            {
                if (value is not hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction0 = castValue;
                return true;
            }
            case "m_friction1":
            case "friction1":
            {
                if (value is not hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction1 = castValue;
                return true;
            }
            case "m_friction2":
            case "friction2":
            {
                if (value is not hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction2 = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not hkpAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            case "m_linLimit0":
            case "linLimit0":
            {
                if (value is not hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit0 = castValue;
                return true;
            }
            case "m_linLimit1":
            case "linLimit1":
            {
                if (value is not hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit1 = castValue;
                return true;
            }
            case "m_linLimit2":
            case "linLimit2":
            {
                if (value is not hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit2 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
