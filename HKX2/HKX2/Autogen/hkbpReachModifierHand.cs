using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpReachModifierHand : IHavokObject
    {
        public virtual uint Signature { get => 2074831058; }
        
        public Vector4 m_targetOrSensingPosition;
        public Vector4 m_targetBackHandNormal;
        public float m_sensingRadius;
        public short m_boneIndex;
        public short m_handIkTrackIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_targetOrSensingPosition = des.ReadVector4(br);
            m_targetBackHandNormal = des.ReadVector4(br);
            m_sensingRadius = br.ReadSingle();
            m_boneIndex = br.ReadInt16();
            m_handIkTrackIndex = br.ReadInt16();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_targetOrSensingPosition);
            s.WriteVector4(bw, m_targetBackHandNormal);
            bw.WriteSingle(m_sensingRadius);
            bw.WriteInt16(m_boneIndex);
            bw.WriteInt16(m_handIkTrackIndex);
            bw.WriteUInt64(0);
        }
    }
}
