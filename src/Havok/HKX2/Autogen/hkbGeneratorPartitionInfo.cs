using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGeneratorPartitionInfo : IHavokObject
    {
        public virtual uint Signature { get => 3318695654; }
        
        public uint m_boneMask_0;
        public uint m_boneMask_1;
        public uint m_boneMask_2;
        public uint m_boneMask_3;
        public uint m_boneMask_4;
        public uint m_boneMask_5;
        public uint m_boneMask_6;
        public uint m_boneMask_7;
        public uint m_partitionMask_0;
        public short m_numBones;
        public short m_numMaxPartitions;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_boneMask_0 = br.ReadUInt32();
            m_boneMask_1 = br.ReadUInt32();
            m_boneMask_2 = br.ReadUInt32();
            m_boneMask_3 = br.ReadUInt32();
            m_boneMask_4 = br.ReadUInt32();
            m_boneMask_5 = br.ReadUInt32();
            m_boneMask_6 = br.ReadUInt32();
            m_boneMask_7 = br.ReadUInt32();
            m_partitionMask_0 = br.ReadUInt32();
            m_numBones = br.ReadInt16();
            m_numMaxPartitions = br.ReadInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_boneMask_0);
            bw.WriteUInt32(m_boneMask_1);
            bw.WriteUInt32(m_boneMask_2);
            bw.WriteUInt32(m_boneMask_3);
            bw.WriteUInt32(m_boneMask_4);
            bw.WriteUInt32(m_boneMask_5);
            bw.WriteUInt32(m_boneMask_6);
            bw.WriteUInt32(m_boneMask_7);
            bw.WriteUInt32(m_partitionMask_0);
            bw.WriteInt16(m_numBones);
            bw.WriteInt16(m_numMaxPartitions);
        }
    }
}
