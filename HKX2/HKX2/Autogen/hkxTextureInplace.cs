using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxTextureInplace : hkReferencedObject
    {
        public override uint Signature { get => 524754443; }
        
        public sbyte m_fileType_0;
        public sbyte m_fileType_1;
        public sbyte m_fileType_2;
        public sbyte m_fileType_3;
        public List<byte> m_data;
        public string m_name;
        public string m_originalFilename;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_fileType_0 = br.ReadSByte();
            m_fileType_1 = br.ReadSByte();
            m_fileType_2 = br.ReadSByte();
            m_fileType_3 = br.ReadSByte();
            br.ReadUInt32();
            m_data = des.ReadByteArray(br);
            m_name = des.ReadStringPointer(br);
            m_originalFilename = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSByte(m_fileType_0);
            bw.WriteSByte(m_fileType_1);
            bw.WriteSByte(m_fileType_2);
            bw.WriteSByte(m_fileType_3);
            bw.WriteUInt32(0);
            s.WriteByteArray(bw, m_data);
            s.WriteStringPointer(bw, m_name);
            s.WriteStringPointer(bw, m_originalFilename);
        }
    }
}
