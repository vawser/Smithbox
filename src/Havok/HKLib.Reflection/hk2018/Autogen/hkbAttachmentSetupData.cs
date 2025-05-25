// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAttachmentSetupData : HavokData<hkbAttachmentSetup> 
{
    public hkbAttachmentSetupData(HavokType type, hkbAttachmentSetup instance) : base(type, instance) {}

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
            case "m_blendInTime":
            case "blendInTime":
            {
                if (instance.m_blendInTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_moveAttacherFraction":
            case "moveAttacherFraction":
            {
                if (instance.m_moveAttacherFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gain":
            case "gain":
            {
                if (instance.m_gain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                if (instance.m_extrapolationTimeStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fixUpGain":
            case "fixUpGain":
            {
                if (instance.m_fixUpGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxLinearDistance":
            case "maxLinearDistance":
            {
                if (instance.m_maxLinearDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngularDistance":
            case "maxAngularDistance":
            {
                if (instance.m_maxAngularDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_attachmentType":
            case "attachmentType":
            {
                if (instance.m_attachmentType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_attachmentType is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
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
                if (value is hkbAttachmentSetup.AttachmentType castValue)
                {
                    instance.m_attachmentType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_attachmentType = (hkbAttachmentSetup.AttachmentType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
