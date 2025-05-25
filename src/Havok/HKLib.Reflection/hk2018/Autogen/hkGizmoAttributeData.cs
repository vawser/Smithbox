// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkGizmoAttributeData : HavokData<hkGizmoAttribute> 
{
    public hkGizmoAttributeData(HavokType type, hkGizmoAttribute instance) : base(type, instance) {}

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
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
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
                if (value is hkGizmoAttribute.GizmoType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkGizmoAttribute.GizmoType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
