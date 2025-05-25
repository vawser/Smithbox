using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSplitGenerationUtilsSettings : IHavokObject
    {
        public virtual uint Signature { get => 1662460593; }
        
        public SplitAndGenerateOptions m_simplificationOptions;
        public SplitMethod m_splitMethod;
        public bool m_generateClusterGraphs;
        public int m_desiredFacesPerCluster;
        public float m_borderPreserveShrinkSize;
        public float m_streamingEdgeMatchTolerance;
        public int m_numX;
        public int m_numY;
        public int m_maxSplits;
        public int m_desiredTrisPerChunk;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_simplificationOptions = (SplitAndGenerateOptions)br.ReadByte();
            m_splitMethod = (SplitMethod)br.ReadByte();
            m_generateClusterGraphs = br.ReadBoolean();
            br.ReadByte();
            m_desiredFacesPerCluster = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            m_borderPreserveShrinkSize = br.ReadSingle();
            m_streamingEdgeMatchTolerance = br.ReadSingle();
            m_numX = br.ReadInt32();
            m_numY = br.ReadInt32();
            m_maxSplits = br.ReadInt32();
            m_desiredTrisPerChunk = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte((byte)m_simplificationOptions);
            bw.WriteByte((byte)m_splitMethod);
            bw.WriteBoolean(m_generateClusterGraphs);
            bw.WriteByte(0);
            bw.WriteInt32(m_desiredFacesPerCluster);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteSingle(m_borderPreserveShrinkSize);
            bw.WriteSingle(m_streamingEdgeMatchTolerance);
            bw.WriteInt32(m_numX);
            bw.WriteInt32(m_numY);
            bw.WriteInt32(m_maxSplits);
            bw.WriteInt32(m_desiredTrisPerChunk);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
