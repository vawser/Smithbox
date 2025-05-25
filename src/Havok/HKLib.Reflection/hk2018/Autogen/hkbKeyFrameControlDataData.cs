// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbKeyFrameControlDataData : HavokData<hkbKeyFrameControlData> 
{
    public hkbKeyFrameControlDataData(HavokType type, hkbKeyFrameControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hierarchyGain":
            case "hierarchyGain":
            {
                if (instance.m_hierarchyGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityDamping":
            case "velocityDamping":
            {
                if (instance.m_velocityDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_accelerationGain":
            case "accelerationGain":
            {
                if (instance.m_accelerationGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityGain":
            case "velocityGain":
            {
                if (instance.m_velocityGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positionGain":
            case "positionGain":
            {
                if (instance.m_positionGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positionMaxLinearVelocity":
            case "positionMaxLinearVelocity":
            {
                if (instance.m_positionMaxLinearVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positionMaxAngularVelocity":
            case "positionMaxAngularVelocity":
            {
                if (instance.m_positionMaxAngularVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapGain":
            case "snapGain":
            {
                if (instance.m_snapGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapMaxLinearVelocity":
            case "snapMaxLinearVelocity":
            {
                if (instance.m_snapMaxLinearVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapMaxAngularVelocity":
            case "snapMaxAngularVelocity":
            {
                if (instance.m_snapMaxAngularVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapMaxLinearDistance":
            case "snapMaxLinearDistance":
            {
                if (instance.m_snapMaxLinearDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapMaxAngularDistance":
            case "snapMaxAngularDistance":
            {
                if (instance.m_snapMaxAngularDistance is not TGet castValue) return false;
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
            case "m_hierarchyGain":
            case "hierarchyGain":
            {
                if (value is not float castValue) return false;
                instance.m_hierarchyGain = castValue;
                return true;
            }
            case "m_velocityDamping":
            case "velocityDamping":
            {
                if (value is not float castValue) return false;
                instance.m_velocityDamping = castValue;
                return true;
            }
            case "m_accelerationGain":
            case "accelerationGain":
            {
                if (value is not float castValue) return false;
                instance.m_accelerationGain = castValue;
                return true;
            }
            case "m_velocityGain":
            case "velocityGain":
            {
                if (value is not float castValue) return false;
                instance.m_velocityGain = castValue;
                return true;
            }
            case "m_positionGain":
            case "positionGain":
            {
                if (value is not float castValue) return false;
                instance.m_positionGain = castValue;
                return true;
            }
            case "m_positionMaxLinearVelocity":
            case "positionMaxLinearVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_positionMaxLinearVelocity = castValue;
                return true;
            }
            case "m_positionMaxAngularVelocity":
            case "positionMaxAngularVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_positionMaxAngularVelocity = castValue;
                return true;
            }
            case "m_snapGain":
            case "snapGain":
            {
                if (value is not float castValue) return false;
                instance.m_snapGain = castValue;
                return true;
            }
            case "m_snapMaxLinearVelocity":
            case "snapMaxLinearVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxLinearVelocity = castValue;
                return true;
            }
            case "m_snapMaxAngularVelocity":
            case "snapMaxAngularVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxAngularVelocity = castValue;
                return true;
            }
            case "m_snapMaxLinearDistance":
            case "snapMaxLinearDistance":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxLinearDistance = castValue;
                return true;
            }
            case "m_snapMaxAngularDistance":
            case "snapMaxAngularDistance":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxAngularDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
