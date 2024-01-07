using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSphereShape : hclShape
    {
        public override uint Signature { get => 3615093445; }
        
        public hkSphere m_sphere;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_sphere = new hkSphere();
            m_sphere.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_sphere.Write(s, bw);
        }
    }
}
