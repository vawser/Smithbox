using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbFootIkGains : IHavokObject
    {
        public virtual uint Signature { get => 3738682663; }
        
        public float m_onOffGain;
        public float m_groundAscendingGain;
        public float m_groundDescendingGain;
        public float m_footPlantedGain;
        public float m_footRaisedGain;
        public float m_footLockingGain;
        public float m_worldFromModelFeedbackGain;
        public float m_errorUpDownBias;
        public float m_alignWorldFromModelGain;
        public float m_hipOrientationGain;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_onOffGain = br.ReadSingle();
            m_groundAscendingGain = br.ReadSingle();
            m_groundDescendingGain = br.ReadSingle();
            m_footPlantedGain = br.ReadSingle();
            m_footRaisedGain = br.ReadSingle();
            m_footLockingGain = br.ReadSingle();
            m_worldFromModelFeedbackGain = br.ReadSingle();
            m_errorUpDownBias = br.ReadSingle();
            m_alignWorldFromModelGain = br.ReadSingle();
            m_hipOrientationGain = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_onOffGain);
            bw.WriteSingle(m_groundAscendingGain);
            bw.WriteSingle(m_groundDescendingGain);
            bw.WriteSingle(m_footPlantedGain);
            bw.WriteSingle(m_footRaisedGain);
            bw.WriteSingle(m_footLockingGain);
            bw.WriteSingle(m_worldFromModelFeedbackGain);
            bw.WriteSingle(m_errorUpDownBias);
            bw.WriteSingle(m_alignWorldFromModelGain);
            bw.WriteSingle(m_hipOrientationGain);
        }
    }
}
