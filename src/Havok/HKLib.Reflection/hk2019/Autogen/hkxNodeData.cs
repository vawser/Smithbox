// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxNodeData : HavokData<HKLib.hk2019.hkxNode> 
{
    public hkxNodeData(HavokType type, HKLib.hk2019.hkxNode instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_attributeGroups":
            case "attributeGroups":
            {
                if (instance.m_attributeGroups is not TGet castValue) return false;
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
            case "m_uuid":
            case "uuid":
            {
                if (instance.m_uuid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_object":
            case "object":
            {
                if (instance.m_object is null)
                {
                    return true;
                }
                if (instance.m_object is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_keyFrames":
            case "keyFrames":
            {
                if (instance.m_keyFrames is not TGet castValue) return false;
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
            case "m_annotations":
            case "annotations":
            {
                if (instance.m_annotations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearKeyFrameHints":
            case "linearKeyFrameHints":
            {
                if (instance.m_linearKeyFrameHints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userProperties":
            case "userProperties":
            {
                if (instance.m_userProperties is null)
                {
                    return true;
                }
                if (instance.m_userProperties is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_selected":
            case "selected":
            {
                if (instance.m_selected is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bone":
            case "bone":
            {
                if (instance.m_bone is not TGet castValue) return false;
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
            case "m_attributeGroups":
            case "attributeGroups":
            {
                if (value is not List<HKLib.hk2019.hkxAttributeGroup> castValue) return false;
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
                if (value is not HKLib.hk2019.hkUuid castValue) return false;
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
                if (value is HKLib.hk2019.hkReferencedObject castValue)
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
                if (value is not List<HKLib.hk2019.hkxNode?> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            case "m_annotations":
            case "annotations":
            {
                if (value is not List<HKLib.hk2019.hkxNode.AnnotationData> castValue) return false;
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
