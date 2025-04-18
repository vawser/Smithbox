using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticPvs : IHavokObject
    {
        public virtual uint Signature { get => 2984005270; }
        
        public hkcdStaticTreeTreehkcdStaticTreeDynamicStorage6 m_cells;
        public int m_bytesPerCells;
        public int m_cellsPerBlock;
        public List<byte> m_pvs;
        public List<ushort> m_map;
        public List<hkcdStaticPvsBlockHeader> m_blocks;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_cells = new hkcdStaticTreeTreehkcdStaticTreeDynamicStorage6();
            m_cells.Read(des, br);
            m_bytesPerCells = br.ReadInt32();
            m_cellsPerBlock = br.ReadInt32();
            m_pvs = des.ReadByteArray(br);
            m_map = des.ReadUInt16Array(br);
            m_blocks = des.ReadClassArray<hkcdStaticPvsBlockHeader>(br);
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_cells.Write(s, bw);
            bw.WriteInt32(m_bytesPerCells);
            bw.WriteInt32(m_cellsPerBlock);
            s.WriteByteArray(bw, m_pvs);
            s.WriteUInt16Array(bw, m_map);
            s.WriteClassArray<hkcdStaticPvsBlockHeader>(bw, m_blocks);
            bw.WriteUInt64(0);
        }
    }
}
