using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStorageExtendedMeshShapeMeshSubpartStorage : hkReferencedObject
    {
        public override uint Signature { get => 4095606354; }
        
        public List<Vector4> m_vertices;
        public List<byte> m_indices8;
        public List<ushort> m_indices16;
        public List<uint> m_indices32;
        public List<byte> m_materialIndices;
        public List<hkpStorageExtendedMeshShapeMaterial> m_materials;
        public List<hkpNamedMeshMaterial> m_namedMaterials;
        public List<ushort> m_materialIndices16;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertices = des.ReadVector4Array(br);
            m_indices8 = des.ReadByteArray(br);
            m_indices16 = des.ReadUInt16Array(br);
            m_indices32 = des.ReadUInt32Array(br);
            m_materialIndices = des.ReadByteArray(br);
            m_materials = des.ReadClassArray<hkpStorageExtendedMeshShapeMaterial>(br);
            m_namedMaterials = des.ReadClassArray<hkpNamedMeshMaterial>(br);
            m_materialIndices16 = des.ReadUInt16Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4Array(bw, m_vertices);
            s.WriteByteArray(bw, m_indices8);
            s.WriteUInt16Array(bw, m_indices16);
            s.WriteUInt32Array(bw, m_indices32);
            s.WriteByteArray(bw, m_materialIndices);
            s.WriteClassArray<hkpStorageExtendedMeshShapeMaterial>(bw, m_materials);
            s.WriteClassArray<hkpNamedMeshMaterial>(bw, m_namedMaterials);
            s.WriteUInt16Array(bw, m_materialIndices16);
        }
    }
}
