using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpDestructionShapeProperties : hkReferencedObject
    {
        public override uint Signature { get => 1594010782; }
        
        public Matrix4x4 m_worldFromShape;
        public bool m_isHierarchicalCompound;
        public bool m_hasDestructionShapes;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_worldFromShape = des.ReadTransform(br);
            m_isHierarchicalCompound = br.ReadBoolean();
            m_hasDestructionShapes = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteTransform(bw, m_worldFromShape);
            bw.WriteBoolean(m_isHierarchicalCompound);
            bw.WriteBoolean(m_hasDestructionShapes);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
