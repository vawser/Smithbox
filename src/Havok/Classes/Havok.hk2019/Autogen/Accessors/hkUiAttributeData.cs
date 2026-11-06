// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkUiAttributeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkUiAttribute>
{
    public hkUiAttributeData(Havok.hk2019.hkUiAttribute instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_visible":
            case "visible":
            {
                value = instance.m_visible;
                return true;
            }
            case "m_editable":
            case "editable":
            {
                value = instance.m_editable;
                return true;
            }
            case "m_hideCriteria":
            case "hideCriteria":
            {
                value = instance.m_hideCriteria;
                return true;
            }
            case "m_label":
            case "label":
            {
                value = instance.m_label;
                return true;
            }
            case "m_group":
            case "group":
            {
                value = instance.m_group;
                return true;
            }
            case "m_hideBaseClassMembers":
            case "hideBaseClassMembers":
            {
                value = instance.m_hideBaseClassMembers;
                return true;
            }
            case "m_endGroup":
            case "endGroup":
            {
                value = instance.m_endGroup;
                return true;
            }
            case "m_endGroup2":
            case "endGroup2":
            {
                value = instance.m_endGroup2;
                return true;
            }
            case "m_advanced":
            case "advanced":
            {
                value = instance.m_advanced;
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
                if (value is Havok.hk2019.hkAttributeHideCriteria.Types castValue)
                {
                    instance.m_hideCriteria = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_hideCriteria = (Havok.hk2019.hkAttributeHideCriteria.Types)sbyteValue;
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
