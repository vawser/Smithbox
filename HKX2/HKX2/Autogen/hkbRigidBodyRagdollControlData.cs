using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRigidBodyRagdollControlData : IHavokObject
    {
        public virtual uint Signature { get => 883256303; }
        
        public hkbKeyFrameControlData m_keyFrameControlData;
        public float m_durationToBlend;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_keyFrameControlData = new hkbKeyFrameControlData();
            m_keyFrameControlData.Read(des, br);
            m_durationToBlend = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_keyFrameControlData.Write(s, bw);
            bw.WriteSingle(m_durationToBlend);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
