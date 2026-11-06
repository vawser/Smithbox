// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxNode>
{
    public hkxNodeData(Havok.hk2018.hkxNode instance) : base(instance) { }

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
            case "m_attributeGroups":
            case "attributeGroups":
            {
                value = instance.m_attributeGroups;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_uuid":
            case "uuid":
            {
                value = instance.m_uuid;
                return true;
            }
            case "m_object":
            case "object":
            {
                value = instance.m_object;
                return true;
            }
            case "m_keyFrames":
            case "keyFrames":
            {
                value = instance.m_keyFrames;
                return true;
            }
            case "m_children":
            case "children":
            {
                value = instance.m_children;
                return true;
            }
            case "m_annotations":
            case "annotations":
            {
                value = instance.m_annotations;
                return true;
            }
            case "m_linearKeyFrameHints":
            case "linearKeyFrameHints":
            {
                value = instance.m_linearKeyFrameHints;
                return true;
            }
            case "m_userProperties":
            case "userProperties":
            {
                value = instance.m_userProperties;
                return true;
            }
            case "m_selected":
            case "selected":
            {
                value = instance.m_selected;
                return true;
            }
            case "m_bone":
            case "bone":
            {
                value = instance.m_bone;
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
            case "m_attributeGroups":
            case "attributeGroups":
            {
                if (value is not List<Havok.hk2018.hkxAttributeGroup> castValue) return false;
                instance.m_attributeGroups = castValue;
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
            case "m_uuid":
            case "uuid":
            {
                if (value is not Havok.hk2018.hkUuid castValue) return false;
                instance.m_uuid = castValue;
                return true;
            }
            case "m_object":
            case "object":
            {
                if (value is null)
                {
                    instance.m_object = default;
                    return true;
                }
                if (value is Havok.hk2018.hkReferencedObject castValue)
                {
                    instance.m_object = castValue;
                    return true;
                }
                return false;
            }
            case "m_keyFrames":
            case "keyFrames":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_keyFrames = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not List<Havok.hk2018.hkxNode?> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            case "m_annotations":
            case "annotations":
            {
                if (value is not List<Havok.hk2018.hkxNode.AnnotationData> castValue) return false;
                instance.m_annotations = castValue;
                return true;
            }
            case "m_linearKeyFrameHints":
            case "linearKeyFrameHints":
            {
                if (value is not List<float> castValue) return false;
                instance.m_linearKeyFrameHints = castValue;
                return true;
            }
            case "m_userProperties":
            case "userProperties":
            {
                if (value is null)
                {
                    instance.m_userProperties = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_userProperties = castValue;
                    return true;
                }
                return false;
            }
            case "m_selected":
            case "selected":
            {
                if (value is not bool castValue) return false;
                instance.m_selected = castValue;
                return true;
            }
            case "m_bone":
            case "bone":
            {
                if (value is not bool castValue) return false;
                instance.m_bone = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
