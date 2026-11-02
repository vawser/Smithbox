// Automatically Generated

namespace HKLib.hk2019;

public class hkxBoneAttachmentData : IHavokObject
{
    public List<hkxBoneAttachmentData.AttachPoint> m_points = new();


    public class AttachPoint : IHavokObject
    {
        public string? m_name;

        public string? m_bone;

        public hkQsTransform m_boneFromLocal = new();

    }

}
