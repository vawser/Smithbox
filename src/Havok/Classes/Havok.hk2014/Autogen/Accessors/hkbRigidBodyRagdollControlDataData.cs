// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbRigidBodyRagdollControlDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbRigidBodyRagdollControlData>
{
    public hkbRigidBodyRagdollControlDataData(Havok.hk2014.hkbRigidBodyRagdollControlData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_keyFrameControlData":
            case "keyFrameControlData":
            {
                value = instance.m_keyFrameControlData;
                return true;
            }
            case "m_durationToBlend":
            case "durationToBlend":
            {
                value = instance.m_durationToBlend;
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
            case "m_keyFrameControlData":
            case "keyFrameControlData":
            {
                if (value is not Havok.hk2014.hkbKeyFrameControlData castValue) return false;
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
