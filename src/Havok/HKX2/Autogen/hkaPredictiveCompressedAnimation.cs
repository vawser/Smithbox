using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum StorageClass
    {
        STORAGE_STATIC = 0,
        STORAGE_REFERENCE = 1,
        STORAGE_DYNAMIC_RANGE = 2,
        STORAGE_DYNAMIC_FIXED = 3,
    }
    
    public enum IntArrayID
    {
        BLOCK_OFFSETS = 0,
        FIRST_FLOAT_BLOCK_OFFSETS = 1,
        IS_ANIMATED_BITMAP = 2,
        IS_FIXED_RANGE_BITMAP = 3,
        DYNAMIC_BONE_TRACK_INDEX = 4,
        DYNAMIC_FLOAT_TRACK_INDEX = 5,
        STATIC_BONE_TRACK_INDEX = 6,
        STATIC_FLOAT_TRACK_INDEX = 7,
        RENORM_QUATERNION_INDEX = 8,
        NUM_INT_ARRAYS = 9,
    }
    
    public enum FloatArrayID
    {
        STATIC_VALUES = 0,
        DYNAMIC_SCALES = 1,
        DYNAMIC_OFFSETS = 2,
        NUM_FLOAT_ARRAYS = 3,
    }
    
    public partial class hkaPredictiveCompressedAnimation : hkaAnimation
    {
        public override uint Signature { get => 3392246962; }
        
        public List<byte> m_compressedData;
        public List<ushort> m_intData;
        public int m_intArrayOffsets_0;
        public int m_intArrayOffsets_1;
        public int m_intArrayOffsets_2;
        public int m_intArrayOffsets_3;
        public int m_intArrayOffsets_4;
        public int m_intArrayOffsets_5;
        public int m_intArrayOffsets_6;
        public int m_intArrayOffsets_7;
        public int m_intArrayOffsets_8;
        public List<float> m_floatData;
        public int m_floatArrayOffsets_0;
        public int m_floatArrayOffsets_1;
        public int m_floatArrayOffsets_2;
        public int m_numBones;
        public int m_numFloatSlots;
        public int m_numFrames;
        public int m_firstFloatBlockScaleAndOffsetIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_compressedData = des.ReadByteArray(br);
            m_intData = des.ReadUInt16Array(br);
            m_intArrayOffsets_0 = br.ReadInt32();
            m_intArrayOffsets_1 = br.ReadInt32();
            m_intArrayOffsets_2 = br.ReadInt32();
            m_intArrayOffsets_3 = br.ReadInt32();
            m_intArrayOffsets_4 = br.ReadInt32();
            m_intArrayOffsets_5 = br.ReadInt32();
            m_intArrayOffsets_6 = br.ReadInt32();
            m_intArrayOffsets_7 = br.ReadInt32();
            m_intArrayOffsets_8 = br.ReadInt32();
            br.ReadUInt32();
            m_floatData = des.ReadSingleArray(br);
            m_floatArrayOffsets_0 = br.ReadInt32();
            m_floatArrayOffsets_1 = br.ReadInt32();
            m_floatArrayOffsets_2 = br.ReadInt32();
            m_numBones = br.ReadInt32();
            m_numFloatSlots = br.ReadInt32();
            m_numFrames = br.ReadInt32();
            m_firstFloatBlockScaleAndOffsetIndex = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteByteArray(bw, m_compressedData);
            s.WriteUInt16Array(bw, m_intData);
            bw.WriteInt32(m_intArrayOffsets_0);
            bw.WriteInt32(m_intArrayOffsets_1);
            bw.WriteInt32(m_intArrayOffsets_2);
            bw.WriteInt32(m_intArrayOffsets_3);
            bw.WriteInt32(m_intArrayOffsets_4);
            bw.WriteInt32(m_intArrayOffsets_5);
            bw.WriteInt32(m_intArrayOffsets_6);
            bw.WriteInt32(m_intArrayOffsets_7);
            bw.WriteInt32(m_intArrayOffsets_8);
            bw.WriteUInt32(0);
            s.WriteSingleArray(bw, m_floatData);
            bw.WriteInt32(m_floatArrayOffsets_0);
            bw.WriteInt32(m_floatArrayOffsets_1);
            bw.WriteInt32(m_floatArrayOffsets_2);
            bw.WriteInt32(m_numBones);
            bw.WriteInt32(m_numFloatSlots);
            bw.WriteInt32(m_numFrames);
            bw.WriteInt32(m_firstFloatBlockScaleAndOffsetIndex);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
