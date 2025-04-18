using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterData : hkReferencedObject
    {
        public override uint Signature { get => 4274285599; }
        
        public hkbCharacterControllerSetup m_characterControllerSetup;
        public Vector4 m_modelUpMS;
        public Vector4 m_modelForwardMS;
        public Vector4 m_modelRightMS;
        public List<hkbVariableInfo> m_characterPropertyInfos;
        public List<int> m_numBonesPerLod;
        public hkbVariableValueSet m_characterPropertyValues;
        public hkbFootIkDriverInfo m_footIkDriverInfo;
        public hkbHandIkDriverInfo m_handIkDriverInfo;
        public hkReferencedObject m_aiControlDriverInfo;
        public hkbCharacterStringData m_stringData;
        public hkbMirroredSkeletonInfo m_mirroredSkeletonInfo;
        public List<short> m_boneAttachmentBoneIndices;
        public List<Matrix4x4> m_boneAttachmentTransforms;
        public float m_scale;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterControllerSetup = new hkbCharacterControllerSetup();
            m_characterControllerSetup.Read(des, br);
            br.ReadUInt64();
            m_modelUpMS = des.ReadVector4(br);
            m_modelForwardMS = des.ReadVector4(br);
            m_modelRightMS = des.ReadVector4(br);
            m_characterPropertyInfos = des.ReadClassArray<hkbVariableInfo>(br);
            m_numBonesPerLod = des.ReadInt32Array(br);
            m_characterPropertyValues = des.ReadClassPointer<hkbVariableValueSet>(br);
            m_footIkDriverInfo = des.ReadClassPointer<hkbFootIkDriverInfo>(br);
            m_handIkDriverInfo = des.ReadClassPointer<hkbHandIkDriverInfo>(br);
            m_aiControlDriverInfo = des.ReadClassPointer<hkReferencedObject>(br);
            m_stringData = des.ReadClassPointer<hkbCharacterStringData>(br);
            m_mirroredSkeletonInfo = des.ReadClassPointer<hkbMirroredSkeletonInfo>(br);
            m_boneAttachmentBoneIndices = des.ReadInt16Array(br);
            m_boneAttachmentTransforms = des.ReadMatrix4Array(br);
            m_scale = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_characterControllerSetup.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_modelUpMS);
            s.WriteVector4(bw, m_modelForwardMS);
            s.WriteVector4(bw, m_modelRightMS);
            s.WriteClassArray<hkbVariableInfo>(bw, m_characterPropertyInfos);
            s.WriteInt32Array(bw, m_numBonesPerLod);
            s.WriteClassPointer<hkbVariableValueSet>(bw, m_characterPropertyValues);
            s.WriteClassPointer<hkbFootIkDriverInfo>(bw, m_footIkDriverInfo);
            s.WriteClassPointer<hkbHandIkDriverInfo>(bw, m_handIkDriverInfo);
            s.WriteClassPointer<hkReferencedObject>(bw, m_aiControlDriverInfo);
            s.WriteClassPointer<hkbCharacterStringData>(bw, m_stringData);
            s.WriteClassPointer<hkbMirroredSkeletonInfo>(bw, m_mirroredSkeletonInfo);
            s.WriteInt16Array(bw, m_boneAttachmentBoneIndices);
            s.WriteMatrix4Array(bw, m_boneAttachmentTransforms);
            bw.WriteSingle(m_scale);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
