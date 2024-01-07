using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpLodShape : hknpCompositeShape
    {
        public override uint Signature { get => 3395502608; }
        
        public int m_numLevelsOfDetail;
        public hknpLodShapeLevelOfDetailInfo m_infos_0;
        public hknpLodShapeLevelOfDetailInfo m_infos_1;
        public hknpLodShapeLevelOfDetailInfo m_infos_2;
        public hknpLodShapeLevelOfDetailInfo m_infos_3;
        public hknpLodShapeLevelOfDetailInfo m_infos_4;
        public hknpLodShapeLevelOfDetailInfo m_infos_5;
        public hknpLodShapeLevelOfDetailInfo m_infos_6;
        public hknpLodShapeLevelOfDetailInfo m_infos_7;
        public hknpShape m_shapes_0;
        public hknpShape m_shapes_1;
        public hknpShape m_shapes_2;
        public hknpShape m_shapes_3;
        public hknpShape m_shapes_4;
        public hknpShape m_shapes_5;
        public hknpShape m_shapes_6;
        public hknpShape m_shapes_7;
        public uint m_shapesMemorySizes_0;
        public uint m_shapesMemorySizes_1;
        public uint m_shapesMemorySizes_2;
        public uint m_shapesMemorySizes_3;
        public uint m_shapesMemorySizes_4;
        public uint m_shapesMemorySizes_5;
        public uint m_shapesMemorySizes_6;
        public uint m_shapesMemorySizes_7;
        public int m_indexCurrentShapeOnSpu;
        public hknpShape m_currentShapePpuAddress;
        public hkAabb m_maximumAabb;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_numLevelsOfDetail = br.ReadInt32();
            m_infos_0 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_0.Read(des, br);
            m_infos_1 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_1.Read(des, br);
            m_infos_2 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_2.Read(des, br);
            m_infos_3 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_3.Read(des, br);
            m_infos_4 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_4.Read(des, br);
            m_infos_5 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_5.Read(des, br);
            m_infos_6 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_6.Read(des, br);
            m_infos_7 = new hknpLodShapeLevelOfDetailInfo();
            m_infos_7.Read(des, br);
            br.ReadUInt32();
            m_shapes_0 = des.ReadClassPointer<hknpShape>(br);
            m_shapes_1 = des.ReadClassPointer<hknpShape>(br);
            m_shapes_2 = des.ReadClassPointer<hknpShape>(br);
            m_shapes_3 = des.ReadClassPointer<hknpShape>(br);
            m_shapes_4 = des.ReadClassPointer<hknpShape>(br);
            m_shapes_5 = des.ReadClassPointer<hknpShape>(br);
            m_shapes_6 = des.ReadClassPointer<hknpShape>(br);
            m_shapes_7 = des.ReadClassPointer<hknpShape>(br);
            m_shapesMemorySizes_0 = br.ReadUInt32();
            m_shapesMemorySizes_1 = br.ReadUInt32();
            m_shapesMemorySizes_2 = br.ReadUInt32();
            m_shapesMemorySizes_3 = br.ReadUInt32();
            m_shapesMemorySizes_4 = br.ReadUInt32();
            m_shapesMemorySizes_5 = br.ReadUInt32();
            m_shapesMemorySizes_6 = br.ReadUInt32();
            m_shapesMemorySizes_7 = br.ReadUInt32();
            m_indexCurrentShapeOnSpu = br.ReadInt32();
            br.ReadUInt32();
            m_currentShapePpuAddress = des.ReadClassPointer<hknpShape>(br);
            br.ReadUInt64();
            m_maximumAabb = new hkAabb();
            m_maximumAabb.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_numLevelsOfDetail);
            m_infos_0.Write(s, bw);
            m_infos_1.Write(s, bw);
            m_infos_2.Write(s, bw);
            m_infos_3.Write(s, bw);
            m_infos_4.Write(s, bw);
            m_infos_5.Write(s, bw);
            m_infos_6.Write(s, bw);
            m_infos_7.Write(s, bw);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_0);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_1);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_2);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_3);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_4);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_5);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_6);
            s.WriteClassPointer<hknpShape>(bw, m_shapes_7);
            bw.WriteUInt32(m_shapesMemorySizes_0);
            bw.WriteUInt32(m_shapesMemorySizes_1);
            bw.WriteUInt32(m_shapesMemorySizes_2);
            bw.WriteUInt32(m_shapesMemorySizes_3);
            bw.WriteUInt32(m_shapesMemorySizes_4);
            bw.WriteUInt32(m_shapesMemorySizes_5);
            bw.WriteUInt32(m_shapesMemorySizes_6);
            bw.WriteUInt32(m_shapesMemorySizes_7);
            bw.WriteInt32(m_indexCurrentShapeOnSpu);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hknpShape>(bw, m_currentShapePpuAddress);
            bw.WriteUInt64(0);
            m_maximumAabb.Write(s, bw);
        }
    }
}
