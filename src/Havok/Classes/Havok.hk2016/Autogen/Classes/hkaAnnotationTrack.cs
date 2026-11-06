// Automatically Generated

namespace Havok.hk2016;

public class hkaAnnotationTrack : IHavokObject
{
    public string? m_trackName;

    public List<hkaAnnotationTrack.Annotation> m_annotations = new();


    public class Annotation : IHavokObject
    {
        public float m_time;

        public string? m_text;

    }

}
