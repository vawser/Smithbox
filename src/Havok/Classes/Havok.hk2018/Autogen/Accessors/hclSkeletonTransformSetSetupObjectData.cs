// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSkeletonTransformSetSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSkeletonTransformSetSetupObject>
{
    public hclSkeletonTransformSetSetupObjectData(Havok.hk2018.hclSkeletonTransformSetSetupObject instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                value = instance.m_skeleton;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                value = instance.m_worldFromModel;
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
            case "m_skeleton":
            case "skeleton":
            {
                if (value is null)
                {
                    instance.m_skeleton = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
