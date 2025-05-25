using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaSkeleton : hkReferencedObject
    {
        public override uint Signature { get => 4274114267; }
        
        public string m_name;
        public List<short> m_parentIndices;
        public List<hkaBone> m_bones;
        public List<Matrix4x4> m_referencePose;
        public List<float> m_referenceFloats;
        public List<string> m_floatSlots;
        public List<hkaSkeletonLocalFrameOnBone> m_localFrames;
        public List<hkaSkeletonPartition> m_partitions;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_parentIndices = des.ReadInt16Array(br);
            m_bones = des.ReadClassArray<hkaBone>(br);
            m_referencePose = des.ReadQSTransformArray(br);
            m_referenceFloats = des.ReadSingleArray(br);
            m_floatSlots = des.ReadStringPointerArray(br);
            m_localFrames = des.ReadClassArray<hkaSkeletonLocalFrameOnBone>(br);
            m_partitions = des.ReadClassArray<hkaSkeletonPartition>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteInt16Array(bw, m_parentIndices);
            s.WriteClassArray<hkaBone>(bw, m_bones);
            s.WriteQSTransformArray(bw, m_referencePose);
            s.WriteSingleArray(bw, m_referenceFloats);
            s.WriteStringPointerArray(bw, m_floatSlots);
            s.WriteClassArray<hkaSkeletonLocalFrameOnBone>(bw, m_localFrames);
            s.WriteClassArray<hkaSkeletonPartition>(bw, m_partitions);
        }
    }
}
