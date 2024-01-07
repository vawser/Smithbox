using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBoneSpaceDeformer : IHavokObject
    {
        public virtual uint Signature { get => 1766829204; }
        
        public enum ControlByte
        {
            FOUR_BLEND = 0,
            THREE_BLEND = 1,
            TWO_BLEND = 2,
            ONE_BLEND = 3,
            NEXT_SPU_BATCH = 4,
        }
        
        public List<hclBoneSpaceDeformerFourBlendEntryBlock> m_fourBlendEntries;
        public List<hclBoneSpaceDeformerThreeBlendEntryBlock> m_threeBlendEntries;
        public List<hclBoneSpaceDeformerTwoBlendEntryBlock> m_twoBlendEntries;
        public List<hclBoneSpaceDeformerOneBlendEntryBlock> m_oneBlendEntries;
        public List<byte> m_controlBytes;
        public ushort m_startVertexIndex;
        public ushort m_endVertexIndex;
        public ushort m_batchSizeSpu;
        public bool m_partialWrite;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_fourBlendEntries = des.ReadClassArray<hclBoneSpaceDeformerFourBlendEntryBlock>(br);
            m_threeBlendEntries = des.ReadClassArray<hclBoneSpaceDeformerThreeBlendEntryBlock>(br);
            m_twoBlendEntries = des.ReadClassArray<hclBoneSpaceDeformerTwoBlendEntryBlock>(br);
            m_oneBlendEntries = des.ReadClassArray<hclBoneSpaceDeformerOneBlendEntryBlock>(br);
            m_controlBytes = des.ReadByteArray(br);
            m_startVertexIndex = br.ReadUInt16();
            m_endVertexIndex = br.ReadUInt16();
            m_batchSizeSpu = br.ReadUInt16();
            m_partialWrite = br.ReadBoolean();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hclBoneSpaceDeformerFourBlendEntryBlock>(bw, m_fourBlendEntries);
            s.WriteClassArray<hclBoneSpaceDeformerThreeBlendEntryBlock>(bw, m_threeBlendEntries);
            s.WriteClassArray<hclBoneSpaceDeformerTwoBlendEntryBlock>(bw, m_twoBlendEntries);
            s.WriteClassArray<hclBoneSpaceDeformerOneBlendEntryBlock>(bw, m_oneBlendEntries);
            s.WriteByteArray(bw, m_controlBytes);
            bw.WriteUInt16(m_startVertexIndex);
            bw.WriteUInt16(m_endVertexIndex);
            bw.WriteUInt16(m_batchSizeSpu);
            bw.WriteBoolean(m_partialWrite);
            bw.WriteByte(0);
        }
    }
}
