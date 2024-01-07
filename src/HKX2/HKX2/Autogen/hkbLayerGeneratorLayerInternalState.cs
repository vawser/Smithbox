using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum FadingState
    {
        FADING_STATE_NONE = 0,
        FADING_STATE_IN = 1,
        FADING_STATE_OUT = 2,
    }
    
    public partial class hkbLayerGeneratorLayerInternalState : IHavokObject
    {
        public virtual uint Signature { get => 3799866964; }
        
        public float m_weight;
        public float m_timeElapsed;
        public float m_onFraction;
        public FadingState m_fadingState;
        public bool m_useMotion;
        public bool m_syncNextFrame;
        public bool m_isActive;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_weight = br.ReadSingle();
            m_timeElapsed = br.ReadSingle();
            m_onFraction = br.ReadSingle();
            m_fadingState = (FadingState)br.ReadSByte();
            m_useMotion = br.ReadBoolean();
            m_syncNextFrame = br.ReadBoolean();
            m_isActive = br.ReadBoolean();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_weight);
            bw.WriteSingle(m_timeElapsed);
            bw.WriteSingle(m_onFraction);
            bw.WriteSByte((sbyte)m_fadingState);
            bw.WriteBoolean(m_useMotion);
            bw.WriteBoolean(m_syncNextFrame);
            bw.WriteBoolean(m_isActive);
        }
    }
}
