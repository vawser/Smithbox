using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclNamedSetupMesh : hclSetupMesh
    {
        public override uint Signature { get => 307094147; }
        
        public string m_name;
        public string m_meshName;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_meshName = des.ReadStringPointer(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteStringPointer(bw, m_meshName);
            bw.WriteUInt64(0);
        }
    }
}
