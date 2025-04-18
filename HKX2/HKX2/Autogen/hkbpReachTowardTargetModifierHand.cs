using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpReachTowardTargetModifierHand : IHavokObject
    {
        public virtual uint Signature { get => 1541759402; }
        
        public short m_shoulderIndex;
        public bool m_isEnabled;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_shoulderIndex = br.ReadInt16();
            m_isEnabled = br.ReadBoolean();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_shoulderIndex);
            bw.WriteBoolean(m_isEnabled);
            bw.WriteByte(0);
        }
    }
}
