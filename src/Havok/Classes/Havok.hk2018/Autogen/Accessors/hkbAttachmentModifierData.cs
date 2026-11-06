// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbAttachmentModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbAttachmentModifier>
{
    public hkbAttachmentModifierData(Havok.hk2018.hkbAttachmentModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_sendToAttacherOnAttach":
            case "sendToAttacherOnAttach":
            {
                value = instance.m_sendToAttacherOnAttach;
                return true;
            }
            case "m_sendToAttacheeOnAttach":
            case "sendToAttacheeOnAttach":
            {
                value = instance.m_sendToAttacheeOnAttach;
                return true;
            }
            case "m_sendToAttacherOnDetach":
            case "sendToAttacherOnDetach":
            {
                value = instance.m_sendToAttacherOnDetach;
                return true;
            }
            case "m_sendToAttacheeOnDetach":
            case "sendToAttacheeOnDetach":
            {
                value = instance.m_sendToAttacheeOnDetach;
                return true;
            }
            case "m_attachmentSetup":
            case "attachmentSetup":
            {
                value = instance.m_attachmentSetup;
                return true;
            }
            case "m_attacherHandle":
            case "attacherHandle":
            {
                value = instance.m_attacherHandle;
                return true;
            }
            case "m_attacheeHandle":
            case "attacheeHandle":
            {
                value = instance.m_attacheeHandle;
                return true;
            }
            case "m_attacheeLayer":
            case "attacheeLayer":
            {
                value = instance.m_attacheeLayer;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_sendToAttacherOnAttach":
            case "sendToAttacherOnAttach":
            {
                if (value is not Havok.hk2018.hkbEventProperty castValue) return false;
                instance.m_sendToAttacherOnAttach = castValue;
                return true;
            }
            case "m_sendToAttacheeOnAttach":
            case "sendToAttacheeOnAttach":
            {
                if (value is not Havok.hk2018.hkbEventProperty castValue) return false;
                instance.m_sendToAttacheeOnAttach = castValue;
                return true;
            }
            case "m_sendToAttacherOnDetach":
            case "sendToAttacherOnDetach":
            {
                if (value is not Havok.hk2018.hkbEventProperty castValue) return false;
                instance.m_sendToAttacherOnDetach = castValue;
                return true;
            }
            case "m_sendToAttacheeOnDetach":
            case "sendToAttacheeOnDetach":
            {
                if (value is not Havok.hk2018.hkbEventProperty castValue) return false;
                instance.m_sendToAttacheeOnDetach = castValue;
                return true;
            }
            case "m_attachmentSetup":
            case "attachmentSetup":
            {
                if (value is null)
                {
                    instance.m_attachmentSetup = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbAttachmentSetup castValue)
                {
                    instance.m_attachmentSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_attacherHandle":
            case "attacherHandle":
            {
                if (value is null)
                {
                    instance.m_attacherHandle = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbHandle castValue)
                {
                    instance.m_attacherHandle = castValue;
                    return true;
                }
                return false;
            }
            case "m_attacheeHandle":
            case "attacheeHandle":
            {
                if (value is null)
                {
                    instance.m_attacheeHandle = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbHandle castValue)
                {
                    instance.m_attacheeHandle = castValue;
                    return true;
                }
                return false;
            }
            case "m_attacheeLayer":
            case "attacheeLayer":
            {
                if (value is not int castValue) return false;
                instance.m_attacheeLayer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
