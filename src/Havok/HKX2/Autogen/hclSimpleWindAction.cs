using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimpleWindAction : hclAction
    {
        public override uint Signature { get => 378250540; }
        
        public Vector4 m_windDirection;
        public float m_windMinSpeed;
        public float m_windMaxSpeed;
        public float m_windFrequency;
        public float m_maximumDrag;
        public Vector4 m_airVelocity;
        public float m_currentTime;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_windDirection = des.ReadVector4(br);
            m_windMinSpeed = br.ReadSingle();
            m_windMaxSpeed = br.ReadSingle();
            m_windFrequency = br.ReadSingle();
            m_maximumDrag = br.ReadSingle();
            m_airVelocity = des.ReadVector4(br);
            m_currentTime = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_windDirection);
            bw.WriteSingle(m_windMinSpeed);
            bw.WriteSingle(m_windMaxSpeed);
            bw.WriteSingle(m_windFrequency);
            bw.WriteSingle(m_maximumDrag);
            s.WriteVector4(bw, m_airVelocity);
            bw.WriteSingle(m_currentTime);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
