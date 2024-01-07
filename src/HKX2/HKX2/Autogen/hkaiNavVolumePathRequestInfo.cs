using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavVolumePathRequestInfo : hkReferencedObject
    {
        public override uint Signature { get => 118576998; }
        
        public hkaiVolumePathfindingUtilFindPathInput m_input;
        public hkaiVolumePathfindingUtilFindPathOutput m_output;
        public int m_priority;
        public bool m_markedForDeletion;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_input = des.ReadClassPointer<hkaiVolumePathfindingUtilFindPathInput>(br);
            m_output = des.ReadClassPointer<hkaiVolumePathfindingUtilFindPathOutput>(br);
            m_priority = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt32();
            m_markedForDeletion = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiVolumePathfindingUtilFindPathInput>(bw, m_input);
            s.WriteClassPointer<hkaiVolumePathfindingUtilFindPathOutput>(bw, m_output);
            bw.WriteInt32(m_priority);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteBoolean(m_markedForDeletion);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
