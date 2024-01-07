using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclCollidable : hkReferencedObject
    {
        public override uint Signature { get => 4047530174; }
        
        public string m_name;
        public Matrix4x4 m_transform;
        public Vector4 m_linearVelocity;
        public Vector4 m_angularVelocity;
        public bool m_pinchDetectionEnabled;
        public sbyte m_pinchDetectionPriority;
        public float m_pinchDetectionRadius;
        public hclShape m_shape;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            br.ReadUInt64();
            m_transform = des.ReadTransform(br);
            m_linearVelocity = des.ReadVector4(br);
            m_angularVelocity = des.ReadVector4(br);
            m_pinchDetectionEnabled = br.ReadBoolean();
            m_pinchDetectionPriority = br.ReadSByte();
            br.ReadUInt16();
            m_pinchDetectionRadius = br.ReadSingle();
            m_shape = des.ReadClassPointer<hclShape>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(0);
            s.WriteTransform(bw, m_transform);
            s.WriteVector4(bw, m_linearVelocity);
            s.WriteVector4(bw, m_angularVelocity);
            bw.WriteBoolean(m_pinchDetectionEnabled);
            bw.WriteSByte(m_pinchDetectionPriority);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_pinchDetectionRadius);
            s.WriteClassPointer<hclShape>(bw, m_shape);
        }
    }
}
