// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxBoneAttachmentDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxBoneAttachmentData>
{
    public hkxBoneAttachmentDataData(Havok.hk2019.hkxBoneAttachmentData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_points":
            case "points":
            {
                value = instance.m_points;
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
            case "m_points":
            case "points":
            {
                if (value is not List<Havok.hk2019.hkxBoneAttachmentData.AttachPoint> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
