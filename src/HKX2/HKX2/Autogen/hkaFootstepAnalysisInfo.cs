using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaFootstepAnalysisInfo : hkReferencedObject
    {
        public override uint Signature { get => 2463109047; }
        
        public List<sbyte> m_name;
        public List<sbyte> m_nameStrike;
        public List<sbyte> m_nameLift;
        public List<sbyte> m_nameLock;
        public List<sbyte> m_nameUnlock;
        public List<float> m_minPos;
        public List<float> m_maxPos;
        public List<float> m_minVel;
        public List<float> m_maxVel;
        public List<float> m_allBonesDown;
        public List<float> m_anyBonesDown;
        public float m_posTol;
        public float m_velTol;
        public float m_duration;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadSByteArray(br);
            m_nameStrike = des.ReadSByteArray(br);
            m_nameLift = des.ReadSByteArray(br);
            m_nameLock = des.ReadSByteArray(br);
            m_nameUnlock = des.ReadSByteArray(br);
            m_minPos = des.ReadSingleArray(br);
            m_maxPos = des.ReadSingleArray(br);
            m_minVel = des.ReadSingleArray(br);
            m_maxVel = des.ReadSingleArray(br);
            m_allBonesDown = des.ReadSingleArray(br);
            m_anyBonesDown = des.ReadSingleArray(br);
            m_posTol = br.ReadSingle();
            m_velTol = br.ReadSingle();
            m_duration = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteSByteArray(bw, m_name);
            s.WriteSByteArray(bw, m_nameStrike);
            s.WriteSByteArray(bw, m_nameLift);
            s.WriteSByteArray(bw, m_nameLock);
            s.WriteSByteArray(bw, m_nameUnlock);
            s.WriteSingleArray(bw, m_minPos);
            s.WriteSingleArray(bw, m_maxPos);
            s.WriteSingleArray(bw, m_minVel);
            s.WriteSingleArray(bw, m_maxVel);
            s.WriteSingleArray(bw, m_allBonesDown);
            s.WriteSingleArray(bw, m_anyBonesDown);
            bw.WriteSingle(m_posTol);
            bw.WriteSingle(m_velTol);
            bw.WriteSingle(m_duration);
            bw.WriteUInt32(0);
        }
    }
}
