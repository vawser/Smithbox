// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxBoneAttachmentDataAttachPointData : HavokData<HKLib.hk2019.hkxBoneAttachmentData.AttachPoint> 
{
    public hkxBoneAttachmentDataAttachPointData(HavokType type, HKLib.hk2019.hkxBoneAttachmentData.AttachPoint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_bone":
            case "bone":
            {
                if (instance.m_bone is null)
                {
                    return true;
                }
                if (instance.m_bone is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneFromLocal":
            case "boneFromLocal":
            {
                if (instance.m_boneFromLocal is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkQsTransform castValue) return false;
                instance.m_boneFromLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
