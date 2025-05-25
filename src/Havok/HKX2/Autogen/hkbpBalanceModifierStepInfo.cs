using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpBalanceModifierStepInfo : IHavokObject
    {
        public virtual uint Signature { get => 2807684598; }
        
        public short m_boneIndex;
        public float m_fractionOfSolution;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_boneIndex = br.ReadInt16();
            br.ReadUInt16();
            m_fractionOfSolution = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_boneIndex);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_fractionOfSolution);
        }
    }
}
