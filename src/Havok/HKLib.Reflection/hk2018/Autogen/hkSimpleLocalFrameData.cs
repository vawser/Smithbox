// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSimpleLocalFrameData : HavokData<hkSimpleLocalFrame> 
{
    public hkSimpleLocalFrameData(HavokType type, hkSimpleLocalFrame instance) : base(type, instance) {}

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
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (instance.m_children is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_parentFrame":
            case "parentFrame":
            {
                if (instance.m_parentFrame is null)
                {
                    return true;
                }
                if (instance.m_parentFrame is TGet castValue)
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not List<hkLocalFrame?> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            case "m_parentFrame":
            case "parentFrame":
            {
                if (value is null)
                {
                    instance.m_parentFrame = default;
                    return true;
                }
                if (value is hkLocalFrame castValue)
                {
                    instance.m_parentFrame = castValue;
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
                if (value is hkLocalFrameGroup castValue)
                {
                    instance.m_group = castValue;
                    return true;
                }
                return false;
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
            default:
            return false;
        }
    }

}
