// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkSimpleLocalFrameData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkSimpleLocalFrame>
{
    public hkSimpleLocalFrameData(Havok.hk2018.hkSimpleLocalFrame instance) : base(instance) { }

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
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_children":
            case "children":
            {
                value = instance.m_children;
                return true;
            }
            case "m_parentFrame":
            case "parentFrame":
            {
                value = instance.m_parentFrame;
                return true;
            }
            case "m_group":
            case "group":
            {
                value = instance.m_group;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
                if (value is not List<Havok.hk2018.hkLocalFrame?> castValue) return false;
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
                if (value is Havok.hk2018.hkLocalFrame castValue)
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
                if (value is Havok.hk2018.hkLocalFrameGroup castValue)
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
