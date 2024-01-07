using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaPredictiveCompressedAnimationTrackCompressionParams : IHavokObject
    {
        public virtual uint Signature { get => 121226737; }
        
        public float m_staticTranslationTolerance;
        public float m_staticRotationTolerance;
        public float m_staticScaleTolerance;
        public float m_staticFloatTolerance;
        public float m_dynamicTranslationTolerance;
        public float m_dynamicRotationTolerance;
        public float m_dynamicScaleTolerance;
        public float m_dynamicFloatTolerance;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_staticTranslationTolerance = br.ReadSingle();
            m_staticRotationTolerance = br.ReadSingle();
            m_staticScaleTolerance = br.ReadSingle();
            m_staticFloatTolerance = br.ReadSingle();
            m_dynamicTranslationTolerance = br.ReadSingle();
            m_dynamicRotationTolerance = br.ReadSingle();
            m_dynamicScaleTolerance = br.ReadSingle();
            m_dynamicFloatTolerance = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_staticTranslationTolerance);
            bw.WriteSingle(m_staticRotationTolerance);
            bw.WriteSingle(m_staticScaleTolerance);
            bw.WriteSingle(m_staticFloatTolerance);
            bw.WriteSingle(m_dynamicTranslationTolerance);
            bw.WriteSingle(m_dynamicRotationTolerance);
            bw.WriteSingle(m_dynamicScaleTolerance);
            bw.WriteSingle(m_dynamicFloatTolerance);
        }
    }
}
