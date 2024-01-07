using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGetUpModifier : hkbModifier
    {
        public override uint Signature { get => 2219940448; }
        
        public Vector4 m_groundNormal;
        public float m_duration;
        public float m_alignWithGroundDuration;
        public short m_rootBoneIndex;
        public short m_otherBoneIndex;
        public short m_anotherBoneIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_groundNormal = des.ReadVector4(br);
            m_duration = br.ReadSingle();
            m_alignWithGroundDuration = br.ReadSingle();
            m_rootBoneIndex = br.ReadInt16();
            m_otherBoneIndex = br.ReadInt16();
            m_anotherBoneIndex = br.ReadInt16();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_groundNormal);
            bw.WriteSingle(m_duration);
            bw.WriteSingle(m_alignWithGroundDuration);
            bw.WriteInt16(m_rootBoneIndex);
            bw.WriteInt16(m_otherBoneIndex);
            bw.WriteInt16(m_anotherBoneIndex);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
        }
    }
}
