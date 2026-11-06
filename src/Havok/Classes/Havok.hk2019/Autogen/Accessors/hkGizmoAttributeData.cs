// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkGizmoAttributeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkGizmoAttribute>
{
    public hkGizmoAttributeData(Havok.hk2019.hkGizmoAttribute instance) : base(instance) { }

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
            case "m_label":
            case "label":
            {
                value = instance.m_label;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkGizmoAttribute.GizmoType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2019.hkGizmoAttribute.GizmoType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
