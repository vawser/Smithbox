using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBoneSpaceDeformerLocalBlockP : IHavokObject
    {
        public virtual uint Signature { get => 4045235075; }
        
        public Vector4 m_localPosition_0;
        public Vector4 m_localPosition_1;
        public Vector4 m_localPosition_2;
        public Vector4 m_localPosition_3;
        public Vector4 m_localPosition_4;
        public Vector4 m_localPosition_5;
        public Vector4 m_localPosition_6;
        public Vector4 m_localPosition_7;
        public Vector4 m_localPosition_8;
        public Vector4 m_localPosition_9;
        public Vector4 m_localPosition_10;
        public Vector4 m_localPosition_11;
        public Vector4 m_localPosition_12;
        public Vector4 m_localPosition_13;
        public Vector4 m_localPosition_14;
        public Vector4 m_localPosition_15;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_localPosition_0 = des.ReadVector4(br);
            m_localPosition_1 = des.ReadVector4(br);
            m_localPosition_2 = des.ReadVector4(br);
            m_localPosition_3 = des.ReadVector4(br);
            m_localPosition_4 = des.ReadVector4(br);
            m_localPosition_5 = des.ReadVector4(br);
            m_localPosition_6 = des.ReadVector4(br);
            m_localPosition_7 = des.ReadVector4(br);
            m_localPosition_8 = des.ReadVector4(br);
            m_localPosition_9 = des.ReadVector4(br);
            m_localPosition_10 = des.ReadVector4(br);
            m_localPosition_11 = des.ReadVector4(br);
            m_localPosition_12 = des.ReadVector4(br);
            m_localPosition_13 = des.ReadVector4(br);
            m_localPosition_14 = des.ReadVector4(br);
            m_localPosition_15 = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_localPosition_0);
            s.WriteVector4(bw, m_localPosition_1);
            s.WriteVector4(bw, m_localPosition_2);
            s.WriteVector4(bw, m_localPosition_3);
            s.WriteVector4(bw, m_localPosition_4);
            s.WriteVector4(bw, m_localPosition_5);
            s.WriteVector4(bw, m_localPosition_6);
            s.WriteVector4(bw, m_localPosition_7);
            s.WriteVector4(bw, m_localPosition_8);
            s.WriteVector4(bw, m_localPosition_9);
            s.WriteVector4(bw, m_localPosition_10);
            s.WriteVector4(bw, m_localPosition_11);
            s.WriteVector4(bw, m_localPosition_12);
            s.WriteVector4(bw, m_localPosition_13);
            s.WriteVector4(bw, m_localPosition_14);
            s.WriteVector4(bw, m_localPosition_15);
        }
    }
}
