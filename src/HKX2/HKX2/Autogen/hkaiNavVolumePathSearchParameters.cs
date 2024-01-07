using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavVolumePathSearchParameters : IHavokObject
    {
        public virtual uint Signature { get => 304199519; }
        
        public enum LineOfSightFlags
        {
            NO_LINE_OF_SIGHT_CHECK = 0,
            EARLY_OUT_IF_NO_COST_MODIFIER = 1,
            EARLY_OUT_ALWAYS = 4,
        }
        
        public Vector4 m_up;
        public byte m_lineOfSightFlags;
        public float m_heuristicWeight;
        public float m_maximumPathLength;
        public hkaiSearchParametersBufferSizes m_bufferSizes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_up = des.ReadVector4(br);
            br.ReadUInt64();
            br.ReadUInt64();
            m_lineOfSightFlags = br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_heuristicWeight = br.ReadSingle();
            m_maximumPathLength = br.ReadSingle();
            m_bufferSizes = new hkaiSearchParametersBufferSizes();
            m_bufferSizes.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_up);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteByte(m_lineOfSightFlags);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_heuristicWeight);
            bw.WriteSingle(m_maximumPathLength);
            m_bufferSizes.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
