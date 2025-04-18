using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpLinearParametricCurve : hkpParametricCurve
    {
        public override uint Signature { get => 1791413813; }
        
        public float m_smoothingFactor;
        public bool m_closedLoop;
        public Vector4 m_dirNotParallelToTangentAlongWholePath;
        public List<Vector4> m_points;
        public List<float> m_distance;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_smoothingFactor = br.ReadSingle();
            m_closedLoop = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
            m_dirNotParallelToTangentAlongWholePath = des.ReadVector4(br);
            m_points = des.ReadVector4Array(br);
            m_distance = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_smoothingFactor);
            bw.WriteBoolean(m_closedLoop);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteVector4(bw, m_dirNotParallelToTangentAlongWholePath);
            s.WriteVector4Array(bw, m_points);
            s.WriteSingleArray(bw, m_distance);
        }
    }
}
