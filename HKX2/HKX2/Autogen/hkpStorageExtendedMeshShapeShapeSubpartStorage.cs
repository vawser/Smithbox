using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStorageExtendedMeshShapeShapeSubpartStorage : hkReferencedObject
    {
        public override uint Signature { get => 3001912805; }
        
        public List<byte> m_materialIndices;
        public List<hkpStorageExtendedMeshShapeMaterial> m_materials;
        public List<ushort> m_materialIndices16;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_materialIndices = des.ReadByteArray(br);
            m_materials = des.ReadClassArray<hkpStorageExtendedMeshShapeMaterial>(br);
            m_materialIndices16 = des.ReadUInt16Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteByteArray(bw, m_materialIndices);
            s.WriteClassArray<hkpStorageExtendedMeshShapeMaterial>(bw, m_materials);
            s.WriteUInt16Array(bw, m_materialIndices16);
        }
    }
}
