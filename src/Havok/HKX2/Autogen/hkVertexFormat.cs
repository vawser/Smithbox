using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ComponentType
    {
        TYPE_NONE = 0,
        TYPE_INT8 = 1,
        TYPE_UINT8 = 2,
        TYPE_INT16 = 3,
        TYPE_UINT16 = 4,
        TYPE_INT32 = 5,
        TYPE_UINT32 = 6,
        TYPE_UINT8_DWORD = 7,
        TYPE_ARGB32 = 8,
        TYPE_FLOAT16 = 9,
        TYPE_FLOAT32 = 10,
        TYPE_VECTOR4 = 11,
        TYPE_LAST = 12,
    }
    
    public enum ComponentUsage
    {
        USAGE_NONE = 0,
        USAGE_POSITION = 1,
        USAGE_NORMAL = 2,
        USAGE_COLOR = 3,
        USAGE_TANGENT = 4,
        USAGE_BINORMAL = 5,
        USAGE_BLEND_MATRIX_INDEX = 6,
        USAGE_BLEND_WEIGHTS = 7,
        USAGE_BLEND_WEIGHTS_LAST_IMPLIED = 8,
        USAGE_TEX_COORD = 9,
        USAGE_POINT_SIZE = 10,
        USAGE_USER = 11,
        USAGE_LAST = 12,
    }
    
    public enum HintFlags
    {
        FLAG_READ = 1,
        FLAG_WRITE = 2,
        FLAG_DYNAMIC = 4,
        FLAG_NOT_SHARED = 8,
    }
    
    public enum SharingType
    {
        SHARING_ALL_SHARED = 0,
        SHARING_ALL_NOT_SHARED = 1,
        SHARING_MIXTURE = 2,
    }
    
    public partial class hkVertexFormat : IHavokObject
    {
        public virtual uint Signature { get => 4045291511; }
        
        public hkVertexFormatElement m_elements_0;
        public hkVertexFormatElement m_elements_1;
        public hkVertexFormatElement m_elements_2;
        public hkVertexFormatElement m_elements_3;
        public hkVertexFormatElement m_elements_4;
        public hkVertexFormatElement m_elements_5;
        public hkVertexFormatElement m_elements_6;
        public hkVertexFormatElement m_elements_7;
        public hkVertexFormatElement m_elements_8;
        public hkVertexFormatElement m_elements_9;
        public hkVertexFormatElement m_elements_10;
        public hkVertexFormatElement m_elements_11;
        public hkVertexFormatElement m_elements_12;
        public hkVertexFormatElement m_elements_13;
        public hkVertexFormatElement m_elements_14;
        public hkVertexFormatElement m_elements_15;
        public hkVertexFormatElement m_elements_16;
        public hkVertexFormatElement m_elements_17;
        public hkVertexFormatElement m_elements_18;
        public hkVertexFormatElement m_elements_19;
        public hkVertexFormatElement m_elements_20;
        public hkVertexFormatElement m_elements_21;
        public hkVertexFormatElement m_elements_22;
        public hkVertexFormatElement m_elements_23;
        public hkVertexFormatElement m_elements_24;
        public hkVertexFormatElement m_elements_25;
        public hkVertexFormatElement m_elements_26;
        public hkVertexFormatElement m_elements_27;
        public hkVertexFormatElement m_elements_28;
        public hkVertexFormatElement m_elements_29;
        public hkVertexFormatElement m_elements_30;
        public hkVertexFormatElement m_elements_31;
        public int m_numElements;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_elements_0 = new hkVertexFormatElement();
            m_elements_0.Read(des, br);
            m_elements_1 = new hkVertexFormatElement();
            m_elements_1.Read(des, br);
            m_elements_2 = new hkVertexFormatElement();
            m_elements_2.Read(des, br);
            m_elements_3 = new hkVertexFormatElement();
            m_elements_3.Read(des, br);
            m_elements_4 = new hkVertexFormatElement();
            m_elements_4.Read(des, br);
            m_elements_5 = new hkVertexFormatElement();
            m_elements_5.Read(des, br);
            m_elements_6 = new hkVertexFormatElement();
            m_elements_6.Read(des, br);
            m_elements_7 = new hkVertexFormatElement();
            m_elements_7.Read(des, br);
            m_elements_8 = new hkVertexFormatElement();
            m_elements_8.Read(des, br);
            m_elements_9 = new hkVertexFormatElement();
            m_elements_9.Read(des, br);
            m_elements_10 = new hkVertexFormatElement();
            m_elements_10.Read(des, br);
            m_elements_11 = new hkVertexFormatElement();
            m_elements_11.Read(des, br);
            m_elements_12 = new hkVertexFormatElement();
            m_elements_12.Read(des, br);
            m_elements_13 = new hkVertexFormatElement();
            m_elements_13.Read(des, br);
            m_elements_14 = new hkVertexFormatElement();
            m_elements_14.Read(des, br);
            m_elements_15 = new hkVertexFormatElement();
            m_elements_15.Read(des, br);
            m_elements_16 = new hkVertexFormatElement();
            m_elements_16.Read(des, br);
            m_elements_17 = new hkVertexFormatElement();
            m_elements_17.Read(des, br);
            m_elements_18 = new hkVertexFormatElement();
            m_elements_18.Read(des, br);
            m_elements_19 = new hkVertexFormatElement();
            m_elements_19.Read(des, br);
            m_elements_20 = new hkVertexFormatElement();
            m_elements_20.Read(des, br);
            m_elements_21 = new hkVertexFormatElement();
            m_elements_21.Read(des, br);
            m_elements_22 = new hkVertexFormatElement();
            m_elements_22.Read(des, br);
            m_elements_23 = new hkVertexFormatElement();
            m_elements_23.Read(des, br);
            m_elements_24 = new hkVertexFormatElement();
            m_elements_24.Read(des, br);
            m_elements_25 = new hkVertexFormatElement();
            m_elements_25.Read(des, br);
            m_elements_26 = new hkVertexFormatElement();
            m_elements_26.Read(des, br);
            m_elements_27 = new hkVertexFormatElement();
            m_elements_27.Read(des, br);
            m_elements_28 = new hkVertexFormatElement();
            m_elements_28.Read(des, br);
            m_elements_29 = new hkVertexFormatElement();
            m_elements_29.Read(des, br);
            m_elements_30 = new hkVertexFormatElement();
            m_elements_30.Read(des, br);
            m_elements_31 = new hkVertexFormatElement();
            m_elements_31.Read(des, br);
            m_numElements = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_elements_0.Write(s, bw);
            m_elements_1.Write(s, bw);
            m_elements_2.Write(s, bw);
            m_elements_3.Write(s, bw);
            m_elements_4.Write(s, bw);
            m_elements_5.Write(s, bw);
            m_elements_6.Write(s, bw);
            m_elements_7.Write(s, bw);
            m_elements_8.Write(s, bw);
            m_elements_9.Write(s, bw);
            m_elements_10.Write(s, bw);
            m_elements_11.Write(s, bw);
            m_elements_12.Write(s, bw);
            m_elements_13.Write(s, bw);
            m_elements_14.Write(s, bw);
            m_elements_15.Write(s, bw);
            m_elements_16.Write(s, bw);
            m_elements_17.Write(s, bw);
            m_elements_18.Write(s, bw);
            m_elements_19.Write(s, bw);
            m_elements_20.Write(s, bw);
            m_elements_21.Write(s, bw);
            m_elements_22.Write(s, bw);
            m_elements_23.Write(s, bw);
            m_elements_24.Write(s, bw);
            m_elements_25.Write(s, bw);
            m_elements_26.Write(s, bw);
            m_elements_27.Write(s, bw);
            m_elements_28.Write(s, bw);
            m_elements_29.Write(s, bw);
            m_elements_30.Write(s, bw);
            m_elements_31.Write(s, bw);
            bw.WriteInt32(m_numElements);
        }
    }
}
