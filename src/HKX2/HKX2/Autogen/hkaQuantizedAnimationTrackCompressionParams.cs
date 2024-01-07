using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaQuantizedAnimationTrackCompressionParams : IHavokObject
    {
        public virtual uint Signature { get => 4158015049; }
        
        public float m_rotationTolerance;
        public float m_translationTolerance;
        public float m_scaleTolerance;
        public float m_floatingTolerance;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_rotationTolerance = br.ReadSingle();
            m_translationTolerance = br.ReadSingle();
            m_scaleTolerance = br.ReadSingle();
            m_floatingTolerance = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_rotationTolerance);
            bw.WriteSingle(m_translationTolerance);
            bw.WriteSingle(m_scaleTolerance);
            bw.WriteSingle(m_floatingTolerance);
        }
    }
}
