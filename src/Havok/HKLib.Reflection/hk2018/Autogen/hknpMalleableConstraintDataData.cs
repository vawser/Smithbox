// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMalleableConstraintDataData : HavokData<hknpMalleableConstraintData> 
{
    public hknpMalleableConstraintDataData(HavokType type, hknpMalleableConstraintData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintData":
            case "constraintData":
            {
                if (instance.m_constraintData is null)
                {
                    return true;
                }
                if (instance.m_constraintData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_atom":
            case "atom":
            {
                if (instance.m_atom is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wantsRuntime":
            case "wantsRuntime":
            {
                if (instance.m_wantsRuntime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                if (instance.m_strength is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_constraintData":
            case "constraintData":
            {
                if (value is null)
                {
                    instance.m_constraintData = default;
                    return true;
                }
                if (value is hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
                    return true;
                }
                return false;
            }
            case "m_atom":
            case "atom":
            {
                if (value is not hknpBridgeConstraintAtom castValue) return false;
                instance.m_atom = castValue;
                return true;
            }
            case "m_wantsRuntime":
            case "wantsRuntime":
            {
                if (value is not bool castValue) return false;
                instance.m_wantsRuntime = castValue;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
