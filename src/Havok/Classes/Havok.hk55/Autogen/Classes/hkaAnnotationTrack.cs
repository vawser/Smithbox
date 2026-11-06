// Automatically Generated

namespace Havok.hk55;

public class hkaAnnotationTrack : IHavokObject
{
    public string? m_name;

    public List<hkaAnnotationTrack.Annotation> m_annotations = new();


    public class Annotation : IHavokObject
    {
        public float m_time;

        public string? m_text;

    }

}
