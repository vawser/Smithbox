// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpLinearClearanceConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpLinearClearanceConstraintData.Atoms>
{
    public hkpLinearClearanceConstraintDataAtomsData(Havok.hk2014.hkpLinearClearanceConstraintData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transforms":
            case "transforms":
            {
                value = instance.m_transforms;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                value = instance.m_motor;
                return true;
            }
            case "m_friction0":
            case "friction0":
            {
                value = instance.m_friction0;
                return true;
            }
            case "m_friction1":
            case "friction1":
            {
                value = instance.m_friction1;
                return true;
            }
            case "m_friction2":
            case "friction2":
            {
                value = instance.m_friction2;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                value = instance.m_ang;
                return true;
            }
            case "m_linLimit0":
            case "linLimit0":
            {
                value = instance.m_linLimit0;
                return true;
            }
            case "m_linLimit1":
            case "linLimit1":
            {
                value = instance.m_linLimit1;
                return true;
            }
            case "m_linLimit2":
            case "linLimit2":
            {
                value = instance.m_linLimit2;
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
            case "m_transforms":
            case "transforms":
            {
                if (value is not Havok.hk2014.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                if (value is not Havok.hk2014.hkpLinMotorConstraintAtom castValue) return false;
                instance.m_motor = castValue;
                return true;
            }
            case "m_friction0":
            case "friction0":
            {
                if (value is not Havok.hk2014.hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction0 = castValue;
                return true;
            }
            case "m_friction1":
            case "friction1":
            {
                if (value is not Havok.hk2014.hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction1 = castValue;
                return true;
            }
            case "m_friction2":
            case "friction2":
            {
                if (value is not Havok.hk2014.hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction2 = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not Havok.hk2014.hkpAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            case "m_linLimit0":
            case "linLimit0":
            {
                if (value is not Havok.hk2014.hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit0 = castValue;
                return true;
            }
            case "m_linLimit1":
            case "linLimit1":
            {
                if (value is not Havok.hk2014.hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit1 = castValue;
                return true;
            }
            case "m_linLimit2":
            case "linLimit2":
            {
                if (value is not Havok.hk2014.hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit2 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
