using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclConvexHeightFieldShape : hclShape
    {
        public override uint Signature { get => 1931996311; }
        
        public ushort m_res;
        public ushort m_resIncBorder;
        public Vector4 m_floatCorrectionOffset;
        public List<byte> m_heights;
        public int m_faces_0;
        public int m_faces_1;
        public int m_faces_2;
        public int m_faces_3;
        public int m_faces_4;
        public int m_faces_5;
        public Matrix4x4 m_localToMapTransform;
        public Vector4 m_localToMapScale;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_res = br.ReadUInt16();
            m_resIncBorder = br.ReadUInt16();
            br.ReadUInt32();
            m_floatCorrectionOffset = des.ReadVector4(br);
            m_heights = des.ReadByteArray(br);
            m_faces_0 = br.ReadInt32();
            m_faces_1 = br.ReadInt32();
            m_faces_2 = br.ReadInt32();
            m_faces_3 = br.ReadInt32();
            m_faces_4 = br.ReadInt32();
            m_faces_5 = br.ReadInt32();
            br.ReadUInt64();
            m_localToMapTransform = des.ReadTransform(br);
            m_localToMapScale = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt16(m_res);
            bw.WriteUInt16(m_resIncBorder);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_floatCorrectionOffset);
            s.WriteByteArray(bw, m_heights);
            bw.WriteInt32(m_faces_0);
            bw.WriteInt32(m_faces_1);
            bw.WriteInt32(m_faces_2);
            bw.WriteInt32(m_faces_3);
            bw.WriteInt32(m_faces_4);
            bw.WriteInt32(m_faces_5);
            bw.WriteUInt64(0);
            s.WriteTransform(bw, m_localToMapTransform);
            s.WriteVector4(bw, m_localToMapScale);
        }
    }
}
