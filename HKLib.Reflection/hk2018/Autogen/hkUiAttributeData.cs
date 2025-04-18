// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkUiAttributeData : HavokData<hkUiAttribute> 
{
    public hkUiAttributeData(HavokType type, hkUiAttribute instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_visible":
            case "visible":
            {
                if (instance.m_visible is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_editable":
            case "editable":
            {
                if (instance.m_editable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hideCriteria":
            case "hideCriteria":
            {
                if (instance.m_hideCriteria is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_hideCriteria is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_label":
            case "label":
            {
                if (instance.m_label is null)
                {
                    return true;
                }
                if (instance.m_label is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_group":
            case "group":
            {
                if (instance.m_group is null)
                {
                    return true;
                }
                if (instance.m_group is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_hideBaseClassMembers":
            case "hideBaseClassMembers":
            {
                if (instance.m_hideBaseClassMembers is null)
                {
                    return true;
                }
                if (instance.m_hideBaseClassMembers is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_endGroup":
            case "endGroup":
            {
                if (instance.m_endGroup is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endGroup2":
            case "endGroup2":
            {
                if (instance.m_endGroup2 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_advanced":
            case "advanced":
            {
                if (instance.m_advanced is not TGet castValue) return false;
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
            case "m_visible":
            case "visible":
            {
                if (value is not bool castValue) return false;
                instance.m_visible = castValue;
                return true;
            }
            case "m_editable":
            case "editable":
            {
                if (value is not bool castValue) return false;
                instance.m_editable = castValue;
                return true;
            }
            case "m_hideCriteria":
            case "hideCriteria":
            {
                if (value is hkAttributeHideCriteria.Types castValue)
                {
                    instance.m_hideCriteria = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_hideCriteria = (hkAttributeHideCriteria.Types)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_label":
            case "label":
            {
                if (value is null)
                {
                    instance.m_label = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_label = castValue;
                    return true;
                }
                return false;
            }
            case "m_group":
            case "group":
            {
                if (value is null)
                {
                    instance.m_group = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_group = castValue;
                    return true;
                }
                return false;
            }
            case "m_hideBaseClassMembers":
            case "hideBaseClassMembers":
            {
                if (value is null)
                {
                    instance.m_hideBaseClassMembers = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_hideBaseClassMembers = castValue;
                    return true;
                }
                return false;
            }
            case "m_endGroup":
            case "endGroup":
            {
                if (value is not bool castValue) return false;
                instance.m_endGroup = castValue;
                return true;
            }
            case "m_endGroup2":
            case "endGroup2":
            {
                if (value is not bool castValue) return false;
                instance.m_endGroup2 = castValue;
                return true;
            }
            case "m_advanced":
            case "advanced":
            {
                if (value is not bool castValue) return false;
                instance.m_advanced = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
