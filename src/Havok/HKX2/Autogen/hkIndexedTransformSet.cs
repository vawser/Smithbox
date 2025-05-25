using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkIndexedTransformSet : hkReferencedObject
    {
        public override uint Signature { get => 735610660; }
        
        public List<Matrix4x4> m_matrices;
        public List<Matrix4x4> m_inverseMatrices;
        public List<short> m_matricesOrder;
        public List<string> m_matricesNames;
        public List<hkMeshBoneIndexMapping> m_indexMappings;
        public bool m_allMatricesAreAffine;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_matrices = des.ReadMatrix4Array(br);
            m_inverseMatrices = des.ReadMatrix4Array(br);
            m_matricesOrder = des.ReadInt16Array(br);
            m_matricesNames = des.ReadStringPointerArray(br);
            m_indexMappings = des.ReadClassArray<hkMeshBoneIndexMapping>(br);
            m_allMatricesAreAffine = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteMatrix4Array(bw, m_matrices);
            s.WriteMatrix4Array(bw, m_inverseMatrices);
            s.WriteInt16Array(bw, m_matricesOrder);
            s.WriteStringPointerArray(bw, m_matricesNames);
            s.WriteClassArray<hkMeshBoneIndexMapping>(bw, m_indexMappings);
            bw.WriteBoolean(m_allMatricesAreAffine);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
