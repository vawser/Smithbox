using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSkeletonTransformSetSetupObject : hclTransformSetSetupObject
    {
        public override uint Signature { get => 2772230430; }
        
        public string m_name;
        public hkaSkeleton m_skeleton;
        public Matrix4x4 m_worldFromModel;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_skeleton = des.ReadClassPointer<hkaSkeleton>(br);
            m_worldFromModel = des.ReadMatrix4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hkaSkeleton>(bw, m_skeleton);
            s.WriteMatrix4(bw, m_worldFromModel);
        }
    }
}
