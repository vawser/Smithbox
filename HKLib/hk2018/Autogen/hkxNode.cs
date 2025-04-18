// Automatically Generated

namespace HKLib.hk2018;

public class hkxNode : hkxAttributeHolder
{
    public string? m_name;

    public hkUuid m_uuid = new();

    public hkReferencedObject? m_object;

    public List<Matrix4x4> m_keyFrames = new();

    public List<hkxNode?> m_children = new();

    public List<hkxNode.AnnotationData> m_annotations = new();

    public List<float> m_linearKeyFrameHints = new();

    public string? m_userProperties;

    public bool m_selected;

    public bool m_bone;


    public class AnnotationData : IHavokObject
    {
        public float m_time;

        public string? m_description;

    }


}

