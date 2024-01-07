using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum CellEdgeFlagBits
    {
        EDGE_EXTERNAL_OPPOSITE = 64,
    }
    
    public partial class hkaiNavVolume : hkReferencedObject
    {
        public override uint Signature { get => 2325863050; }
        
        public enum Constants
        {
            INVALID_CELL_INDEX = -1,
            INVALID_EDGE_INDEX = -1,
        }
        
        public List<hkaiNavVolumeCell> m_cells;
        public List<hkaiNavVolumeEdge> m_edges;
        public List<hkaiStreamingSet> m_streamingSets;
        public hkAabb m_aabb;
        public Vector4 m_quantizationScale;
        public Vector4 m_quantizationOffset;
        public int m_res_0;
        public int m_res_1;
        public int m_res_2;
        public ulong m_userData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_cells = des.ReadClassArray<hkaiNavVolumeCell>(br);
            m_edges = des.ReadClassArray<hkaiNavVolumeEdge>(br);
            m_streamingSets = des.ReadClassArray<hkaiStreamingSet>(br);
            m_aabb = new hkAabb();
            m_aabb.Read(des, br);
            m_quantizationScale = des.ReadVector4(br);
            m_quantizationOffset = des.ReadVector4(br);
            m_res_0 = br.ReadInt32();
            m_res_1 = br.ReadInt32();
            m_res_2 = br.ReadInt32();
            br.ReadUInt32();
            m_userData = br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiNavVolumeCell>(bw, m_cells);
            s.WriteClassArray<hkaiNavVolumeEdge>(bw, m_edges);
            s.WriteClassArray<hkaiStreamingSet>(bw, m_streamingSets);
            m_aabb.Write(s, bw);
            s.WriteVector4(bw, m_quantizationScale);
            s.WriteVector4(bw, m_quantizationOffset);
            bw.WriteInt32(m_res_0);
            bw.WriteInt32(m_res_1);
            bw.WriteInt32(m_res_2);
            bw.WriteUInt32(0);
            bw.WriteUInt64(m_userData);
            bw.WriteUInt64(0);
        }
    }
}
