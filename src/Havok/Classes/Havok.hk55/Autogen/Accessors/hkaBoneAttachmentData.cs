// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaBoneAttachmentData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaBoneAttachment>
{
    public hkaBoneAttachmentData(Havok.hk55.hkaBoneAttachment instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneFromAttachment":
            case "boneFromAttachment":
            {
                value = instance.m_boneFromAttachment;
                return true;
            }
            case "m_attachment":
            case "attachment":
            {
                value = instance.m_attachment;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                value = instance.m_boneIndex;
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
                if (value is not Havok.hk55.hkVariant castValue) return false;
                instance.m_attachment = castValue;
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
