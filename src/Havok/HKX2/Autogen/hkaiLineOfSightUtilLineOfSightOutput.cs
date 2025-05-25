using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiLineOfSightUtilLineOfSightOutput : IHavokObject
    {
        public virtual uint Signature { get => 3729143435; }
        
        public List<uint> m_visitedEdgesOut;
        public List<float> m_distancesOut;
        public List<Vector4> m_pointsOut;
        public bool m_doNotExceedArrayCapacity;
        public int m_numIterationsOut;
        public uint m_finalFaceKey;
        public float m_accumulatedDistance;
        public Vector4 m_finalPoint;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_visitedEdgesOut = des.ReadUInt32Array(br);
            m_distancesOut = des.ReadSingleArray(br);
            m_pointsOut = des.ReadVector4Array(br);
            m_doNotExceedArrayCapacity = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_numIterationsOut = br.ReadInt32();
            m_finalFaceKey = br.ReadUInt32();
            m_accumulatedDistance = br.ReadSingle();
            m_finalPoint = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteUInt32Array(bw, m_visitedEdgesOut);
            s.WriteSingleArray(bw, m_distancesOut);
            s.WriteVector4Array(bw, m_pointsOut);
            bw.WriteBoolean(m_doNotExceedArrayCapacity);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_numIterationsOut);
            bw.WriteUInt32(m_finalFaceKey);
            bw.WriteSingle(m_accumulatedDistance);
            s.WriteVector4(bw, m_finalPoint);
        }
    }
}
