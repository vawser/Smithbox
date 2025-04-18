using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum RotationQuantization
    {
        POLAR32 = 0,
        THREECOMP40 = 1,
        THREECOMP48 = 2,
        THREECOMP24 = 3,
        STRAIGHT16 = 4,
        UNCOMPRESSED = 5,
    }
    
    public enum ScalarQuantization
    {
        BITS8 = 0,
        BITS16 = 1,
    }
    
    public partial class hkaSplineCompressedAnimationTrackCompressionParams : IHavokObject
    {
        public virtual uint Signature { get => 1122531539; }
        
        public float m_rotationTolerance;
        public float m_translationTolerance;
        public float m_scaleTolerance;
        public float m_floatingTolerance;
        public ushort m_rotationDegree;
        public ushort m_translationDegree;
        public ushort m_scaleDegree;
        public ushort m_floatingDegree;
        public RotationQuantization m_rotationQuantizationType;
        public ScalarQuantization m_translationQuantizationType;
        public ScalarQuantization m_scaleQuantizationType;
        public ScalarQuantization m_floatQuantizationType;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_rotationTolerance = br.ReadSingle();
            m_translationTolerance = br.ReadSingle();
            m_scaleTolerance = br.ReadSingle();
            m_floatingTolerance = br.ReadSingle();
            m_rotationDegree = br.ReadUInt16();
            m_translationDegree = br.ReadUInt16();
            m_scaleDegree = br.ReadUInt16();
            m_floatingDegree = br.ReadUInt16();
            m_rotationQuantizationType = (RotationQuantization)br.ReadByte();
            m_translationQuantizationType = (ScalarQuantization)br.ReadByte();
            m_scaleQuantizationType = (ScalarQuantization)br.ReadByte();
            m_floatQuantizationType = (ScalarQuantization)br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_rotationTolerance);
            bw.WriteSingle(m_translationTolerance);
            bw.WriteSingle(m_scaleTolerance);
            bw.WriteSingle(m_floatingTolerance);
            bw.WriteUInt16(m_rotationDegree);
            bw.WriteUInt16(m_translationDegree);
            bw.WriteUInt16(m_scaleDegree);
            bw.WriteUInt16(m_floatingDegree);
            bw.WriteByte((byte)m_rotationQuantizationType);
            bw.WriteByte((byte)m_translationQuantizationType);
            bw.WriteByte((byte)m_scaleQuantizationType);
            bw.WriteByte((byte)m_floatQuantizationType);
        }
    }
}
