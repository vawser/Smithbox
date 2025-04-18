// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaBoneAttachmentData : HavokData<hkaBoneAttachment> 
{
    public hkaBoneAttachmentData(HavokType type, hkaBoneAttachment instance) : base(type, instance) {}

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
            case "m_originalSkeletonName":
            case "originalSkeletonName":
            {
                if (instance.m_originalSkeletonName is null)
                {
                    return true;
                }
                if (instance.m_originalSkeletonName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneFromAttachment":
            case "boneFromAttachment":
            {
                if (instance.m_boneFromAttachment is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_attachment":
            case "attachment":
            {
                if (instance.m_attachment is null)
                {
                    return true;
                }
                if (instance.m_attachment is TGet castValue)
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
            case "m_boneIndex":
            case "boneIndex":
            {
                if (instance.m_boneIndex is not TGet castValue) return false;
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
            case "m_originalSkeletonName":
            case "originalSkeletonName":
            {
                if (value is null)
                {
                    instance.m_originalSkeletonName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_originalSkeletonName = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneFromAttachment":
            case "boneFromAttachment":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_boneFromAttachment = castValue;
                return true;
            }
            case "m_attachment":
            case "attachment":
            {
                if (value is null)
                {
                    instance.m_attachment = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_attachment = castValue;
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
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
