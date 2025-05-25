using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclTaperedCapsuleShape : hclShape
    {
        public override uint Signature { get => 3752169249; }
        
        public Vector4 m_small;
        public Vector4 m_big;
        public Vector4 m_coneApex;
        public Vector4 m_coneAxis;
        public Vector4 m_lVec;
        public Vector4 m_dVec;
        public Vector4 m_tanThetaVecNeg;
        public float m_smallRadius;
        public float m_bigRadius;
        public float m_l;
        public float m_d;
        public float m_cosTheta;
        public float m_sinTheta;
        public float m_tanTheta;
        public float m_tanThetaSqr;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_small = des.ReadVector4(br);
            m_big = des.ReadVector4(br);
            m_coneApex = des.ReadVector4(br);
            m_coneAxis = des.ReadVector4(br);
            m_lVec = des.ReadVector4(br);
            m_dVec = des.ReadVector4(br);
            m_tanThetaVecNeg = des.ReadVector4(br);
            m_smallRadius = br.ReadSingle();
            m_bigRadius = br.ReadSingle();
            m_l = br.ReadSingle();
            m_d = br.ReadSingle();
            m_cosTheta = br.ReadSingle();
            m_sinTheta = br.ReadSingle();
            m_tanTheta = br.ReadSingle();
            m_tanThetaSqr = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_small);
            s.WriteVector4(bw, m_big);
            s.WriteVector4(bw, m_coneApex);
            s.WriteVector4(bw, m_coneAxis);
            s.WriteVector4(bw, m_lVec);
            s.WriteVector4(bw, m_dVec);
            s.WriteVector4(bw, m_tanThetaVecNeg);
            bw.WriteSingle(m_smallRadius);
            bw.WriteSingle(m_bigRadius);
            bw.WriteSingle(m_l);
            bw.WriteSingle(m_d);
            bw.WriteSingle(m_cosTheta);
            bw.WriteSingle(m_sinTheta);
            bw.WriteSingle(m_tanTheta);
            bw.WriteSingle(m_tanThetaSqr);
        }
    }
}
