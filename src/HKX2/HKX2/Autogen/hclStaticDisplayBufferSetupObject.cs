using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStaticDisplayBufferSetupObject : hclBufferSetupObject
    {
        public override uint Signature { get => 4232738643; }
        
        public hclSetupMesh m_setupMesh;
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_setupMesh = des.ReadClassPointer<hclSetupMesh>(br);
            m_name = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hclSetupMesh>(bw, m_setupMesh);
            s.WriteStringPointer(bw, m_name);
        }
    }
}
