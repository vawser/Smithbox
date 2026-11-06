// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpPrismaticConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpPrismaticConstraintData.Atoms>
{
    public hkpPrismaticConstraintDataAtomsData(Havok.hk2019.hkpPrismaticConstraintData.Atoms instance) : base(instance) { }

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
            case "m_friction":
            case "friction":
            {
                value = instance.m_friction;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                value = instance.m_ang;
                return true;
            }
            case "m_lin0":
            case "lin0":
            {
                value = instance.m_lin0;
                return true;
            }
            case "m_lin1":
            case "lin1":
            {
                value = instance.m_lin1;
                return true;
            }
            case "m_linLimit":
            case "linLimit":
            {
                value = instance.m_linLimit;
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
                if (value is not Havok.hk2019.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                if (value is not Havok.hk2019.hkpLinMotorConstraintAtom castValue) return false;
                instance.m_motor = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not Havok.hk2019.hkpLinFrictionConstraintAtom castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not Havok.hk2019.hkpAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            case "m_lin0":
            case "lin0":
            {
                if (value is not Havok.hk2019.hkpLinConstraintAtom castValue) return false;
                instance.m_lin0 = castValue;
                return true;
            }
            case "m_lin1":
            case "lin1":
            {
                if (value is not Havok.hk2019.hkpLinConstraintAtom castValue) return false;
                instance.m_lin1 = castValue;
                return true;
            }
            case "m_linLimit":
            case "linLimit":
            {
                if (value is not Havok.hk2019.hkpLinLimitConstraintAtom castValue) return false;
                instance.m_linLimit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
