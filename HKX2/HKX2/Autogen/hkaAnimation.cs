using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum AnimationType
    {
        HK_UNKNOWN_ANIMATION = 0,
        HK_INTERLEAVED_ANIMATION = 1,
        HK_MIRRORED_ANIMATION = 2,
        HK_SPLINE_COMPRESSED_ANIMATION = 3,
        HK_QUANTIZED_COMPRESSED_ANIMATION = 4,
        HK_PREDICTIVE_COMPRESSED_ANIMATION = 5,
        HK_REFERENCE_POSE_ANIMATION = 6,
    }
    
    public partial class hkaAnimation : hkReferencedObject
    {
        public override uint Signature { get => 3041263128; }
        
        public AnimationType m_type;
        public float m_duration;
        public int m_numberOfTransformTracks;
        public int m_numberOfFloatTracks;
        public hkaAnimatedReferenceFrame m_extractedMotion;
        public List<hkaAnnotationTrack> m_annotationTracks;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_type = (AnimationType)br.ReadInt32();
            m_duration = br.ReadSingle();
            m_numberOfTransformTracks = br.ReadInt32();
            m_numberOfFloatTracks = br.ReadInt32();
            m_extractedMotion = des.ReadClassPointer<hkaAnimatedReferenceFrame>(br);
            m_annotationTracks = des.ReadClassArray<hkaAnnotationTrack>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32((int)m_type);
            bw.WriteSingle(m_duration);
            bw.WriteInt32(m_numberOfTransformTracks);
            bw.WriteInt32(m_numberOfFloatTracks);
            s.WriteClassPointer<hkaAnimatedReferenceFrame>(bw, m_extractedMotion);
            s.WriteClassArray<hkaAnnotationTrack>(bw, m_annotationTracks);
        }
    }
}
