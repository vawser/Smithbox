// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRigidBodyRagdollControlDataData : HavokData<hkbRigidBodyRagdollControlData> 
{
    public hkbRigidBodyRagdollControlDataData(HavokType type, hkbRigidBodyRagdollControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_keyFrameControlData":
            case "keyFrameControlData":
            {
                if (instance.m_keyFrameControlData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_durationToBlend":
            case "durationToBlend":
            {
                if (instance.m_durationToBlend is not TGet castValue) return false;
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
            case "m_keyFrameControlData":
            case "keyFrameControlData":
            {
                if (value is not hkbKeyFrameControlData castValue) return false;
                instance.m_keyFrameControlData = castValue;
                return true;
            }
            case "m_durationToBlend":
            case "durationToBlend":
            {
                if (value is not float castValue) return false;
                instance.m_durationToBlend = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
