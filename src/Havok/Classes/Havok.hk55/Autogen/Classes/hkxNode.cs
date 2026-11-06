// Automatically Generated

namespace Havok.hk55;

public class hkxNode : hkxAttributeHolder
{
    public string? m_name;

    public hkVariant m_object = new();

    public List<Matrix4x4> m_keyFrames = new();

    public List<hkxNode?> m_children = new();

    public List<hkxNode.AnnotationData> m_annotations = new();

    public string? m_userProperties;

    public bool m_selected;


    public class AnnotationData : IHavokObject
    {
        public float m_time;

        public string? m_description;

    }

}
