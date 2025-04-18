using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimClothPose : hkReferencedObject
    {
        public override uint Signature { get => 455429281; }
        
        public string m_name;
        public List<Vector4> m_positions;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_positions = des.ReadVector4Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteVector4Array(bw, m_positions);
        }
    }
}
