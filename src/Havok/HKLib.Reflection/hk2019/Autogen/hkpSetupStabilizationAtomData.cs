// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpSetupStabilizationAtomData : HavokData<HKLib.hk2019.hkpSetupStabilizationAtom> 
{
    public hkpSetupStabilizationAtomData(HavokType type, HKLib.hk2019.hkpSetupStabilizationAtom instance) : base(type, instance) {}

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
            case "m_enabled":
            case "enabled":
            {
                if (instance.m_enabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxLinImpulse":
            case "maxLinImpulse":
            {
                if (instance.m_maxLinImpulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngImpulse":
            case "maxAngImpulse":
            {
                if (instance.m_maxAngImpulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (instance.m_maxAngle is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (HKLib.hk2019.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_enabled":
            case "enabled":
            {
                if (value is not bool castValue) return false;
                instance.m_enabled = castValue;
                return true;
            }
            case "m_maxLinImpulse":
            case "maxLinImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_maxLinImpulse = castValue;
                return true;
            }
            case "m_maxAngImpulse":
            case "maxAngImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngImpulse = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
