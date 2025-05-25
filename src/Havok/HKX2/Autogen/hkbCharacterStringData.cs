using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterStringData : hkReferencedObject
    {
        public override uint Signature { get => 194873938; }
        
        public List<hkbCharacterStringDataFileNameMeshNamePair> m_skinNames;
        public List<hkbCharacterStringDataFileNameMeshNamePair> m_boneAttachmentNames;
        public List<hkbAssetBundleStringData> m_animationBundleNameData;
        public List<hkbAssetBundleStringData> m_animationBundleFilenameData;
        public List<string> m_characterPropertyNames;
        public List<string> m_retargetingSkeletonMapperFilenames;
        public List<string> m_lodNames;
        public List<string> m_mirroredSyncPointSubstringsA;
        public List<string> m_mirroredSyncPointSubstringsB;
        public string m_name;
        public string m_rigName;
        public string m_ragdollName;
        public string m_behaviorFilename;
        public string m_luaScriptOnCharacterActivated;
        public string m_luaScriptOnCharacterDeactivated;
        public List<string> m_luaFiles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_skinNames = des.ReadClassArray<hkbCharacterStringDataFileNameMeshNamePair>(br);
            m_boneAttachmentNames = des.ReadClassArray<hkbCharacterStringDataFileNameMeshNamePair>(br);
            m_animationBundleNameData = des.ReadClassArray<hkbAssetBundleStringData>(br);
            m_animationBundleFilenameData = des.ReadClassArray<hkbAssetBundleStringData>(br);
            m_characterPropertyNames = des.ReadStringPointerArray(br);
            m_retargetingSkeletonMapperFilenames = des.ReadStringPointerArray(br);
            m_lodNames = des.ReadStringPointerArray(br);
            m_mirroredSyncPointSubstringsA = des.ReadStringPointerArray(br);
            m_mirroredSyncPointSubstringsB = des.ReadStringPointerArray(br);
            m_name = des.ReadStringPointer(br);
            m_rigName = des.ReadStringPointer(br);
            m_ragdollName = des.ReadStringPointer(br);
            m_behaviorFilename = des.ReadStringPointer(br);
            m_luaScriptOnCharacterActivated = des.ReadStringPointer(br);
            m_luaScriptOnCharacterDeactivated = des.ReadStringPointer(br);
            m_luaFiles = des.ReadStringPointerArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbCharacterStringDataFileNameMeshNamePair>(bw, m_skinNames);
            s.WriteClassArray<hkbCharacterStringDataFileNameMeshNamePair>(bw, m_boneAttachmentNames);
            s.WriteClassArray<hkbAssetBundleStringData>(bw, m_animationBundleNameData);
            s.WriteClassArray<hkbAssetBundleStringData>(bw, m_animationBundleFilenameData);
            s.WriteStringPointerArray(bw, m_characterPropertyNames);
            s.WriteStringPointerArray(bw, m_retargetingSkeletonMapperFilenames);
            s.WriteStringPointerArray(bw, m_lodNames);
            s.WriteStringPointerArray(bw, m_mirroredSyncPointSubstringsA);
            s.WriteStringPointerArray(bw, m_mirroredSyncPointSubstringsB);
            s.WriteStringPointer(bw, m_name);
            s.WriteStringPointer(bw, m_rigName);
            s.WriteStringPointer(bw, m_ragdollName);
            s.WriteStringPointer(bw, m_behaviorFilename);
            s.WriteStringPointer(bw, m_luaScriptOnCharacterActivated);
            s.WriteStringPointer(bw, m_luaScriptOnCharacterDeactivated);
            s.WriteStringPointerArray(bw, m_luaFiles);
        }
    }
}
