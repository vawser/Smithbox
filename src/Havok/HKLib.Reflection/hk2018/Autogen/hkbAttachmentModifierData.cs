// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAttachmentModifierData : HavokData<hkbAttachmentModifier> 
{
    public hkbAttachmentModifierData(HavokType type, hkbAttachmentModifier instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sendToAttacherOnAttach":
            case "sendToAttacherOnAttach":
            {
                if (instance.m_sendToAttacherOnAttach is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sendToAttacheeOnAttach":
            case "sendToAttacheeOnAttach":
            {
                if (instance.m_sendToAttacheeOnAttach is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sendToAttacherOnDetach":
            case "sendToAttacherOnDetach":
            {
                if (instance.m_sendToAttacherOnDetach is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sendToAttacheeOnDetach":
            case "sendToAttacheeOnDetach":
            {
                if (instance.m_sendToAttacheeOnDetach is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_attachmentSetup":
            case "attachmentSetup":
            {
                if (instance.m_attachmentSetup is null)
                {
                    return true;
                }
                if (instance.m_attachmentSetup is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_attacherHandle":
            case "attacherHandle":
            {
                if (instance.m_attacherHandle is null)
                {
                    return true;
                }
                if (instance.m_attacherHandle is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_attacheeHandle":
            case "attacheeHandle":
            {
                if (instance.m_attacheeHandle is null)
                {
                    return true;
                }
                if (instance.m_attacheeHandle is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_attacheeLayer":
            case "attacheeLayer":
            {
                if (instance.m_attacheeLayer is not TGet castValue) return false;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
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
                if (value is not hkbEventProperty castValue) return false;
                instance.m_sendToAttacherOnAttach = castValue;
                return true;
            }
            case "m_sendToAttacheeOnAttach":
            case "sendToAttacheeOnAttach":
            {
                if (value is not hkbEventProperty castValue) return false;
                instance.m_sendToAttacheeOnAttach = castValue;
                return true;
            }
            case "m_sendToAttacherOnDetach":
            case "sendToAttacherOnDetach":
            {
                if (value is not hkbEventProperty castValue) return false;
                instance.m_sendToAttacherOnDetach = castValue;
                return true;
            }
            case "m_sendToAttacheeOnDetach":
            case "sendToAttacheeOnDetach":
            {
                if (value is not hkbEventProperty castValue) return false;
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
                if (value is hkbAttachmentSetup castValue)
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
                if (value is hkbHandle castValue)
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
                if (value is hkbHandle castValue)
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
