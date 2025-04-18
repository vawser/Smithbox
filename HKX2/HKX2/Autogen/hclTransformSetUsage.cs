using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclTransformSetUsage : IHavokObject
    {
        public virtual uint Signature { get => 3212874625; }
        
        public enum Component
        {
            COMPONENT_TRANSFORM = 0,
            COMPONENT_INVTRANSPOSE = 1,
            NUM_COMPONENTS = 2,
        }
        
        public enum InternalFlags
        {
            USAGE_NONE = 0,
            USAGE_READ = 1,
            USAGE_WRITE = 2,
            USAGE_FULL_WRITE = 4,
            USAGE_READ_BEFORE_WRITE = 8,
        }
        
        public byte m_perComponentFlags_0;
        public byte m_perComponentFlags_1;
        public List<hclTransformSetUsageTransformTracker> m_perComponentTransformTrackers;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_perComponentFlags_0 = br.ReadByte();
            m_perComponentFlags_1 = br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            m_perComponentTransformTrackers = des.ReadClassArray<hclTransformSetUsageTransformTracker>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(m_perComponentFlags_0);
            bw.WriteByte(m_perComponentFlags_1);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteClassArray<hclTransformSetUsageTransformTracker>(bw, m_perComponentTransformTrackers);
        }
    }
}
