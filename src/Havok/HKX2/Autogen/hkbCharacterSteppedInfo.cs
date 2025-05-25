using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterSteppedInfo : hkReferencedObject
    {
        public override uint Signature { get => 13372306; }
        
        public ulong m_characterId;
        public float m_deltaTime;
        public Matrix4x4 m_worldFromModel;
        public List<Matrix4x4> m_poseModelSpace;
        public List<Matrix4x4> m_rigidAttachmentTransforms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_deltaTime = br.ReadSingle();
            br.ReadUInt32();
            m_worldFromModel = des.ReadQSTransform(br);
            m_poseModelSpace = des.ReadQSTransformArray(br);
            m_rigidAttachmentTransforms = des.ReadQSTransformArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            bw.WriteSingle(m_deltaTime);
            bw.WriteUInt32(0);
            s.WriteQSTransform(bw, m_worldFromModel);
            s.WriteQSTransformArray(bw, m_poseModelSpace);
            s.WriteQSTransformArray(bw, m_rigidAttachmentTransforms);
        }
    }
}
