// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxBoneAttachmentDataAttachPointData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxBoneAttachmentData.AttachPoint>
{
    public hkxBoneAttachmentDataAttachPointData(Havok.hk2019.hkxBoneAttachmentData.AttachPoint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_bone":
            case "bone":
            {
                value = instance.m_bone;
                return true;
            }
            case "m_boneFromLocal":
            case "boneFromLocal":
            {
                value = instance.m_boneFromLocal;
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
            case "m_bone":
            case "bone":
            {
                if (value is null)
                {
                    instance.m_bone = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_bone = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneFromLocal":
            case "boneFromLocal":
            {
                if (value is not Havok.hk2019.hkQsTransform castValue) return false;
                instance.m_boneFromLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
