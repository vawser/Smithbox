using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpMeshShapeSubpart : IHavokObject
    {
        public virtual uint Signature { get => 657682013; }
        
        public int m_vertexStriding;
        public int m_numVertices;
        public MeshShapeIndexStridingType m_stridingType;
        public MeshShapeMaterialIndexStridingType m_materialIndexStridingType;
        public int m_indexStriding;
        public int m_flipAlternateTriangles;
        public int m_numTriangles;
        public int m_materialIndexStriding;
        public int m_materialStriding;
        public int m_numMaterials;
        public int m_triangleOffset;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadUInt64();
            m_vertexStriding = br.ReadInt32();
            m_numVertices = br.ReadInt32();
            br.ReadUInt64();
            m_stridingType = (MeshShapeIndexStridingType)br.ReadSByte();
            m_materialIndexStridingType = (MeshShapeMaterialIndexStridingType)br.ReadSByte();
            br.ReadUInt16();
            m_indexStriding = br.ReadInt32();
            m_flipAlternateTriangles = br.ReadInt32();
            m_numTriangles = br.ReadInt32();
            br.ReadUInt64();
            m_materialIndexStriding = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt32();
            m_materialStriding = br.ReadInt32();
            m_numMaterials = br.ReadInt32();
            m_triangleOffset = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt64(0);
            bw.WriteInt32(m_vertexStriding);
            bw.WriteInt32(m_numVertices);
            bw.WriteUInt64(0);
            bw.WriteSByte((sbyte)m_stridingType);
            bw.WriteSByte((sbyte)m_materialIndexStridingType);
            bw.WriteUInt16(0);
            bw.WriteInt32(m_indexStriding);
            bw.WriteInt32(m_flipAlternateTriangles);
            bw.WriteInt32(m_numTriangles);
            bw.WriteUInt64(0);
            bw.WriteInt32(m_materialIndexStriding);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteInt32(m_materialStriding);
            bw.WriteInt32(m_numMaterials);
            bw.WriteInt32(m_triangleOffset);
            bw.WriteUInt32(0);
        }
    }
}
