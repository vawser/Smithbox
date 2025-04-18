using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbContext : IHavokObject
    {
        public virtual uint Signature { get => 3427916116; }
        
        public hkbBehaviorGraph m_rootBehavior;
        public hkbGeneratorOutputListener m_generatorOutputListener;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadUInt64();
            m_rootBehavior = des.ReadClassPointer<hkbBehaviorGraph>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_generatorOutputListener = des.ReadClassPointer<hkbGeneratorOutputListener>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkbBehaviorGraph>(bw, m_rootBehavior);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkbGeneratorOutputListener>(bw, m_generatorOutputListener);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
