using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCustomTestGeneratorBoneTypes : hkbCustomTestGeneratorNestedTypes
    {
        public override uint Signature { get => 801687986; }
        
        public bool m_boneHiddenTypeCopyStart;
        public short m_oldBoneIndex;
        public short m_oldBoneIndexNoVar;
        public short m_boneIndex;
        public short m_boneIndexNoVar;
        public short m_boneChainIndex0;
        public short m_boneChainIndex1;
        public short m_boneChainIndex2;
        public short m_boneContractIndex0;
        public short m_boneContractIndex1;
        public short m_boneContractIndex2;
        public bool m_boneHiddenTypeCopyEnd;
        public hkbBoneWeightArray m_boneWeightArray;
        public hkbBoneIndexArray m_boneIndexArray;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boneHiddenTypeCopyStart = br.ReadBoolean();
            br.ReadByte();
            m_oldBoneIndex = br.ReadInt16();
            m_oldBoneIndexNoVar = br.ReadInt16();
            m_boneIndex = br.ReadInt16();
            m_boneIndexNoVar = br.ReadInt16();
            m_boneChainIndex0 = br.ReadInt16();
            m_boneChainIndex1 = br.ReadInt16();
            m_boneChainIndex2 = br.ReadInt16();
            m_boneContractIndex0 = br.ReadInt16();
            m_boneContractIndex1 = br.ReadInt16();
            m_boneContractIndex2 = br.ReadInt16();
            m_boneHiddenTypeCopyEnd = br.ReadBoolean();
            br.ReadByte();
            m_boneWeightArray = des.ReadClassPointer<hkbBoneWeightArray>(br);
            m_boneIndexArray = des.ReadClassPointer<hkbBoneIndexArray>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_boneHiddenTypeCopyStart);
            bw.WriteByte(0);
            bw.WriteInt16(m_oldBoneIndex);
            bw.WriteInt16(m_oldBoneIndexNoVar);
            bw.WriteInt16(m_boneIndex);
            bw.WriteInt16(m_boneIndexNoVar);
            bw.WriteInt16(m_boneChainIndex0);
            bw.WriteInt16(m_boneChainIndex1);
            bw.WriteInt16(m_boneChainIndex2);
            bw.WriteInt16(m_boneContractIndex0);
            bw.WriteInt16(m_boneContractIndex1);
            bw.WriteInt16(m_boneContractIndex2);
            bw.WriteBoolean(m_boneHiddenTypeCopyEnd);
            bw.WriteByte(0);
            s.WriteClassPointer<hkbBoneWeightArray>(bw, m_boneWeightArray);
            s.WriteClassPointer<hkbBoneIndexArray>(bw, m_boneIndexArray);
            bw.WriteUInt64(0);
        }
    }
}
