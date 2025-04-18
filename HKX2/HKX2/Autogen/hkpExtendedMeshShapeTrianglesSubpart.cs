using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpExtendedMeshShapeTrianglesSubpart : hkpExtendedMeshShapeSubpart
    {
        public override uint Signature { get => 667919576; }
        
        public int m_numTriangleShapes;
        public int m_numVertices;
        public ushort m_vertexStriding;
        public int m_triangleOffset;
        public ushort m_indexStriding;
        public IndexStridingType m_stridingType;
        public sbyte m_flipAlternateTriangles;
        public Vector4 m_extrusion;
        public Matrix4x4 m_transform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_numTriangleShapes = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt32();
            m_numVertices = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt32();
            m_vertexStriding = br.ReadUInt16();
            br.ReadUInt16();
            m_triangleOffset = br.ReadInt32();
            m_indexStriding = br.ReadUInt16();
            m_stridingType = (IndexStridingType)br.ReadSByte();
            m_flipAlternateTriangles = br.ReadSByte();
            br.ReadUInt32();
            m_extrusion = des.ReadVector4(br);
            m_transform = des.ReadQSTransform(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_numTriangleShapes);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteInt32(m_numVertices);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(m_vertexStriding);
            bw.WriteUInt16(0);
            bw.WriteInt32(m_triangleOffset);
            bw.WriteUInt16(m_indexStriding);
            bw.WriteSByte((sbyte)m_stridingType);
            bw.WriteSByte(m_flipAlternateTriangles);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_extrusion);
            s.WriteQSTransform(bw, m_transform);
        }
    }
}
