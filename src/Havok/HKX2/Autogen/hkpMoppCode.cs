using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BuildType
    {
        BUILT_WITH_CHUNK_SUBDIVISION = 0,
        BUILT_WITHOUT_CHUNK_SUBDIVISION = 1,
        BUILD_NOT_SET = 2,
    }
    
    public partial class hkpMoppCode : hkReferencedObject
    {
        public override uint Signature { get => 1359281533; }
        
        public hkpMoppCodeCodeInfo m_info;
        public List<byte> m_data;
        public BuildType m_buildType;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_info = new hkpMoppCodeCodeInfo();
            m_info.Read(des, br);
            m_data = des.ReadByteArray(br);
            m_buildType = (BuildType)br.ReadSByte();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_info.Write(s, bw);
            s.WriteByteArray(bw, m_data);
            bw.WriteSByte((sbyte)m_buildType);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
