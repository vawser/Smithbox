using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaAnnotationTrackAnnotation : IHavokObject
    {
        public virtual uint Signature { get => 1648096079; }
        
        public float m_time;
        public string m_text;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_time = br.ReadSingle();
            br.ReadUInt32();
            m_text = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_time);
            bw.WriteUInt32(0);
            s.WriteStringPointer(bw, m_text);
        }
    }
}
