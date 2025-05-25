using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaSplineCompressedAnimation : hkaAnimation
    {
        public override uint Signature { get => 2352701310; }
        
        public int m_numFrames;
        public int m_numBlocks;
        public int m_maxFramesPerBlock;
        public int m_maskAndQuantizationSize;
        public float m_blockDuration;
        public float m_blockInverseDuration;
        public float m_frameDuration;
        public List<uint> m_blockOffsets;
        public List<uint> m_floatBlockOffsets;
        public List<uint> m_transformOffsets;
        public List<uint> m_floatOffsets;
        public List<byte> m_data;
        public int m_endian;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_numFrames = br.ReadInt32();
            m_numBlocks = br.ReadInt32();
            m_maxFramesPerBlock = br.ReadInt32();
            m_maskAndQuantizationSize = br.ReadInt32();
            m_blockDuration = br.ReadSingle();
            m_blockInverseDuration = br.ReadSingle();
            m_frameDuration = br.ReadSingle();
            br.ReadUInt32();
            m_blockOffsets = des.ReadUInt32Array(br);
            m_floatBlockOffsets = des.ReadUInt32Array(br);
            m_transformOffsets = des.ReadUInt32Array(br);
            m_floatOffsets = des.ReadUInt32Array(br);
            m_data = des.ReadByteArray(br);
            m_endian = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_numFrames);
            bw.WriteInt32(m_numBlocks);
            bw.WriteInt32(m_maxFramesPerBlock);
            bw.WriteInt32(m_maskAndQuantizationSize);
            bw.WriteSingle(m_blockDuration);
            bw.WriteSingle(m_blockInverseDuration);
            bw.WriteSingle(m_frameDuration);
            bw.WriteUInt32(0);
            s.WriteUInt32Array(bw, m_blockOffsets);
            s.WriteUInt32Array(bw, m_floatBlockOffsets);
            s.WriteUInt32Array(bw, m_transformOffsets);
            s.WriteUInt32Array(bw, m_floatOffsets);
            s.WriteByteArray(bw, m_data);
            bw.WriteInt32(m_endian);
            bw.WriteUInt32(0);
        }
    }
}
