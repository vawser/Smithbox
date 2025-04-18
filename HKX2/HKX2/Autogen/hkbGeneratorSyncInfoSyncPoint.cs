using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGeneratorSyncInfoSyncPoint : IHavokObject
    {
        public virtual uint Signature { get => 3046625170; }
        
        public int m_id;
        public float m_time;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_id = br.ReadInt32();
            m_time = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_id);
            bw.WriteSingle(m_time);
        }
    }
}
