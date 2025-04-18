using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxBlobMeshShape : hkMeshShape
    {
        public override uint Signature { get => 1704013991; }
        
        public hkxBlob m_blob;
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_blob = new hkxBlob();
            m_blob.Read(des, br);
            m_name = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_blob.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
        }
    }
}
