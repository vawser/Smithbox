using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpCatchFallModifierHand : IHavokObject
    {
        public virtual uint Signature { get => 3906340000; }
        
        public short m_animShoulderIndex;
        public short m_ragdollShoulderIndex;
        public short m_ragdollAnkleIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_animShoulderIndex = br.ReadInt16();
            m_ragdollShoulderIndex = br.ReadInt16();
            m_ragdollAnkleIndex = br.ReadInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_animShoulderIndex);
            bw.WriteInt16(m_ragdollShoulderIndex);
            bw.WriteInt16(m_ragdollAnkleIndex);
        }
    }
}
