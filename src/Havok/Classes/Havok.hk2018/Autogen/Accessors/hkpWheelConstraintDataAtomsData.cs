// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpWheelConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpWheelConstraintData.Atoms>
{
    public hkpWheelConstraintDataAtomsData(Havok.hk2018.hkpWheelConstraintData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_suspensionBase":
            case "suspensionBase":
            {
                value = instance.m_suspensionBase;
                return true;
            }
            case "m_lin0Limit":
            case "lin0Limit":
            {
                value = instance.m_lin0Limit;
                return true;
            }
            case "m_lin0Soft":
            case "lin0Soft":
            {
                value = instance.m_lin0Soft;
                return true;
            }
            case "m_lin1":
            case "lin1":
            {
                value = instance.m_lin1;
                return true;
            }
            case "m_lin2":
            case "lin2":
            {
                value = instance.m_lin2;
                return true;
            }
            case "m_steeringBase":
            case "steeringBase":
            {
                value = instance.m_steeringBase;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                value = instance.m_2dAng;
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
            case "m_suspensionBase":
            case "suspensionBase":
            {
                if (value is not Havok.hk2018.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_suspensionBase = castValue;
                return true;
            }
            case "m_lin0Limit":
            case "lin0Limit":
            {
                if (value is not Havok.hk2018.hkpLinLimitConstraintAtom castValue) return false;
                instance.m_lin0Limit = castValue;
                return true;
            }
            case "m_lin0Soft":
            case "lin0Soft":
            {
                if (value is not Havok.hk2018.hkpLinSoftConstraintAtom castValue) return false;
                instance.m_lin0Soft = castValue;
                return true;
            }
            case "m_lin1":
            case "lin1":
            {
                if (value is not Havok.hk2018.hkpLinConstraintAtom castValue) return false;
                instance.m_lin1 = castValue;
                return true;
            }
            case "m_lin2":
            case "lin2":
            {
                if (value is not Havok.hk2018.hkpLinConstraintAtom castValue) return false;
                instance.m_lin2 = castValue;
                return true;
            }
            case "m_steeringBase":
            case "steeringBase":
            {
                if (value is not Havok.hk2018.hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_steeringBase = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (value is not Havok.hk2018.hkp2dAngConstraintAtom castValue) return false;
                instance.m_2dAng = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
