using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MotionSpaceType
    {
        MST_UNKNOWN = 0,
        MST_ANGULAR = 1,
        MST_DIRECTIONAL = 2,
    }
    
    public partial class hkbParametricMotionGenerator : hkbProceduralBlenderGenerator
    {
        public override uint Signature { get => 2366506881; }
        
        public MotionSpaceType m_motionSpace;
        public List<hkbGenerator> m_generators;
        public float m_xAxisParameterValue;
        public float m_yAxisParameterValue;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_motionSpace = (MotionSpaceType)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_generators = des.ReadClassPointerArray<hkbGenerator>(br);
            m_xAxisParameterValue = br.ReadSingle();
            m_yAxisParameterValue = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSByte((sbyte)m_motionSpace);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointerArray<hkbGenerator>(bw, m_generators);
            bw.WriteSingle(m_xAxisParameterValue);
            bw.WriteSingle(m_yAxisParameterValue);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
