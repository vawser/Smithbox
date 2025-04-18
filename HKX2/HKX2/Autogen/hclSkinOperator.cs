using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BONE_GROUP_SIZE
    {
        GROUP_SIZE_1 = 1,
        GROUP_SIZE_4 = 4,
        GROUP_SIZE_8 = 8,
        GROUP_SIZE_16 = 16,
    }
    
    public partial class hclSkinOperator : hclOperator
    {
        public override uint Signature { get => 3522357854; }
        
        public List<hclSkinOperatorBoneInfluence> m_boneInfluences;
        public List<ushort> m_boneInfluenceStartPerVertex;
        public List<Matrix4x4> m_boneFromSkinMeshTransforms;
        public List<ushort> m_usedBoneGroupIds;
        public bool m_skinPositions;
        public bool m_skinNormals;
        public bool m_skinTangents;
        public bool m_skinBiTangents;
        public uint m_inputBufferIndex;
        public uint m_outputBufferIndex;
        public uint m_transformSetIndex;
        public ushort m_startVertex;
        public ushort m_endVertex;
        public bool m_partialSkinning;
        public bool m_dualQuaternionSkinning;
        public byte m_boneGroupSize;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boneInfluences = des.ReadClassArray<hclSkinOperatorBoneInfluence>(br);
            m_boneInfluenceStartPerVertex = des.ReadUInt16Array(br);
            m_boneFromSkinMeshTransforms = des.ReadMatrix4Array(br);
            m_usedBoneGroupIds = des.ReadUInt16Array(br);
            m_skinPositions = br.ReadBoolean();
            m_skinNormals = br.ReadBoolean();
            m_skinTangents = br.ReadBoolean();
            m_skinBiTangents = br.ReadBoolean();
            m_inputBufferIndex = br.ReadUInt32();
            m_outputBufferIndex = br.ReadUInt32();
            m_transformSetIndex = br.ReadUInt32();
            m_startVertex = br.ReadUInt16();
            m_endVertex = br.ReadUInt16();
            m_partialSkinning = br.ReadBoolean();
            m_dualQuaternionSkinning = br.ReadBoolean();
            m_boneGroupSize = br.ReadByte();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclSkinOperatorBoneInfluence>(bw, m_boneInfluences);
            s.WriteUInt16Array(bw, m_boneInfluenceStartPerVertex);
            s.WriteMatrix4Array(bw, m_boneFromSkinMeshTransforms);
            s.WriteUInt16Array(bw, m_usedBoneGroupIds);
            bw.WriteBoolean(m_skinPositions);
            bw.WriteBoolean(m_skinNormals);
            bw.WriteBoolean(m_skinTangents);
            bw.WriteBoolean(m_skinBiTangents);
            bw.WriteUInt32(m_inputBufferIndex);
            bw.WriteUInt32(m_outputBufferIndex);
            bw.WriteUInt32(m_transformSetIndex);
            bw.WriteUInt16(m_startVertex);
            bw.WriteUInt16(m_endVertex);
            bw.WriteBoolean(m_partialSkinning);
            bw.WriteBoolean(m_dualQuaternionSkinning);
            bw.WriteByte(m_boneGroupSize);
            bw.WriteByte(0);
        }
    }
}
