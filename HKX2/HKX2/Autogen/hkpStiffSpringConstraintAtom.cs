using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStiffSpringConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 520373280; }
        
        public float m_length;
        public float m_maxLength;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt16();
            m_length = br.ReadSingle();
            m_maxLength = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_length);
            bw.WriteSingle(m_maxLength);
        }
    }
}
