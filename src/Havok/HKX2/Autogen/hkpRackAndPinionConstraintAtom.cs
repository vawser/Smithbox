using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpRackAndPinionConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 2821035908; }
        
        public float m_pinionRadiusOrScrewPitch;
        public bool m_isScrew;
        public sbyte m_memOffsetToInitialAngleOffset;
        public sbyte m_memOffsetToPrevAngle;
        public sbyte m_memOffsetToRevolutionCounter;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt16();
            m_pinionRadiusOrScrewPitch = br.ReadSingle();
            m_isScrew = br.ReadBoolean();
            m_memOffsetToInitialAngleOffset = br.ReadSByte();
            m_memOffsetToPrevAngle = br.ReadSByte();
            m_memOffsetToRevolutionCounter = br.ReadSByte();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_pinionRadiusOrScrewPitch);
            bw.WriteBoolean(m_isScrew);
            bw.WriteSByte(m_memOffsetToInitialAngleOffset);
            bw.WriteSByte(m_memOffsetToPrevAngle);
            bw.WriteSByte(m_memOffsetToRevolutionCounter);
            bw.WriteUInt32(0);
        }
    }
}
