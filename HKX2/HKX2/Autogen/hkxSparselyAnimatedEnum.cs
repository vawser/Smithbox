using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxSparselyAnimatedEnum : hkxSparselyAnimatedInt
    {
        public override uint Signature { get => 1214579830; }
        
        public hkxEnum m_enum;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_enum = des.ReadClassPointer<hkxEnum>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkxEnum>(bw, m_enum);
        }
    }
}
