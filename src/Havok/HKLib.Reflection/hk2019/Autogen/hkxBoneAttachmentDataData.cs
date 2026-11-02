// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxBoneAttachmentDataData : HavokData<HKLib.hk2019.hkxBoneAttachmentData> 
{
    public hkxBoneAttachmentDataData(HavokType type, HKLib.hk2019.hkxBoneAttachmentData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_points":
            case "points":
            {
                if (instance.m_points is not TGet castValue) return false;
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
            case "m_points":
            case "points":
            {
                if (value is not List<HKLib.hk2019.hkxBoneAttachmentData.AttachPoint> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
