// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpWheelConstraintDataAtomsData : HavokData<hkpWheelConstraintData.Atoms> 
{
    public hkpWheelConstraintDataAtomsData(HavokType type, hkpWheelConstraintData.Atoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_suspensionBase":
            case "suspensionBase":
            {
                if (instance.m_suspensionBase is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lin0Limit":
            case "lin0Limit":
            {
                if (instance.m_lin0Limit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lin0Soft":
            case "lin0Soft":
            {
                if (instance.m_lin0Soft is not TGet castValue) return false;
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
            case "m_lin2":
            case "lin2":
            {
                if (instance.m_lin2 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_steeringBase":
            case "steeringBase":
            {
                if (instance.m_steeringBase is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (instance.m_2dAng is not TGet castValue) return false;
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
            case "m_suspensionBase":
            case "suspensionBase":
            {
                if (value is not hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_suspensionBase = castValue;
                return true;
            }
            case "m_lin0Limit":
            case "lin0Limit":
            {
                if (value is not hkpLinLimitConstraintAtom castValue) return false;
                instance.m_lin0Limit = castValue;
                return true;
            }
            case "m_lin0Soft":
            case "lin0Soft":
            {
                if (value is not hkpLinSoftConstraintAtom castValue) return false;
                instance.m_lin0Soft = castValue;
                return true;
            }
            case "m_lin1":
            case "lin1":
            {
                if (value is not hkpLinConstraintAtom castValue) return false;
                instance.m_lin1 = castValue;
                return true;
            }
            case "m_lin2":
            case "lin2":
            {
                if (value is not hkpLinConstraintAtom castValue) return false;
                instance.m_lin2 = castValue;
                return true;
            }
            case "m_steeringBase":
            case "steeringBase":
            {
                if (value is not hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_steeringBase = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (value is not hkp2dAngConstraintAtom castValue) return false;
                instance.m_2dAng = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
