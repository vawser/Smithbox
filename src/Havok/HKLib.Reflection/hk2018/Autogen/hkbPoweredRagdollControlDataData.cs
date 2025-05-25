// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbPoweredRagdollControlDataData : HavokData<hkbPoweredRagdollControlData> 
{
    public hkbPoweredRagdollControlDataData(HavokType type, hkbPoweredRagdollControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxForce":
            case "maxForce":
            {
                if (instance.m_maxForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                if (instance.m_tau is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (instance.m_damping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_proportionalRecoveryVelocity":
            case "proportionalRecoveryVelocity":
            {
                if (instance.m_proportionalRecoveryVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constantRecoveryVelocity":
            case "constantRecoveryVelocity":
            {
                if (instance.m_constantRecoveryVelocity is not TGet castValue) return false;
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
            case "m_maxForce":
            case "maxForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxForce = castValue;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                if (value is not float castValue) return false;
                instance.m_tau = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_proportionalRecoveryVelocity":
            case "proportionalRecoveryVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_proportionalRecoveryVelocity = castValue;
                return true;
            }
            case "m_constantRecoveryVelocity":
            case "constantRecoveryVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_constantRecoveryVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
