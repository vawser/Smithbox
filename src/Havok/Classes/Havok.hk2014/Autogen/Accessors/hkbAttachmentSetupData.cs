// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbAttachmentSetupData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbAttachmentSetup>
{
    public hkbAttachmentSetupData(Havok.hk2014.hkbAttachmentSetup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blendInTime":
            case "blendInTime":
            {
                value = instance.m_blendInTime;
                return true;
            }
            case "m_moveAttacherFraction":
            case "moveAttacherFraction":
            {
                value = instance.m_moveAttacherFraction;
                return true;
            }
            case "m_gain":
            case "gain":
            {
                value = instance.m_gain;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                value = instance.m_extrapolationTimeStep;
                return true;
            }
            case "m_fixUpGain":
            case "fixUpGain":
            {
                value = instance.m_fixUpGain;
                return true;
            }
            case "m_maxLinearDistance":
            case "maxLinearDistance":
            {
                value = instance.m_maxLinearDistance;
                return true;
            }
            case "m_maxAngularDistance":
            case "maxAngularDistance":
            {
                value = instance.m_maxAngularDistance;
                return true;
            }
            case "m_attachmentType":
            case "attachmentType":
            {
                value = instance.m_attachmentType;
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
            case "m_blendInTime":
            case "blendInTime":
            {
                if (value is not float castValue) return false;
                instance.m_blendInTime = castValue;
                return true;
            }
            case "m_moveAttacherFraction":
            case "moveAttacherFraction":
            {
                if (value is not float castValue) return false;
                instance.m_moveAttacherFraction = castValue;
                return true;
            }
            case "m_gain":
            case "gain":
            {
                if (value is not float castValue) return false;
                instance.m_gain = castValue;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                if (value is not float castValue) return false;
                instance.m_extrapolationTimeStep = castValue;
                return true;
            }
            case "m_fixUpGain":
            case "fixUpGain":
            {
                if (value is not float castValue) return false;
                instance.m_fixUpGain = castValue;
                return true;
            }
            case "m_maxLinearDistance":
            case "maxLinearDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxLinearDistance = castValue;
                return true;
            }
            case "m_maxAngularDistance":
            case "maxAngularDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngularDistance = castValue;
                return true;
            }
            case "m_attachmentType":
            case "attachmentType":
            {
                if (value is Havok.hk2014.hkbAttachmentSetup.AttachmentType castValue)
                {
                    instance.m_attachmentType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_attachmentType = (Havok.hk2014.hkbAttachmentSetup.AttachmentType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
