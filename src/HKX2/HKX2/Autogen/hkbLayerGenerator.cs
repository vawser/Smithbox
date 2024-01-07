using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum LayerFlagBits
    {
        FLAG_SYNC = 1,
    }
    
    public partial class hkbLayerGenerator : hkbGenerator
    {
        public override uint Signature { get => 3034629423; }
        
        public List<hkbLayer> m_layers;
        public short m_indexOfSyncMasterChild;
        public ushort m_flags;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_layers = des.ReadClassPointerArray<hkbLayer>(br);
            m_indexOfSyncMasterChild = br.ReadInt16();
            m_flags = br.ReadUInt16();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbLayer>(bw, m_layers);
            bw.WriteInt16(m_indexOfSyncMasterChild);
            bw.WriteUInt16(m_flags);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
