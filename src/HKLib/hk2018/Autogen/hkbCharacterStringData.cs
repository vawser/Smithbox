// Automatically Generated

namespace HKLib.hk2018;

public class hkbCharacterStringData : hkReferencedObject
{
    public List<hkbCharacterStringData.FileNameMeshNamePair> m_skinNames = new();

    public List<hkbCharacterStringData.FileNameMeshNamePair> m_boneAttachmentNames = new();

    public List<hkbAssetBundleStringData> m_animationBundleNameData = new();

    public List<hkbAssetBundleStringData> m_animationBundleFilenameData = new();

    public List<string?> m_characterPropertyNames = new();

    public List<string?> m_retargetingSkeletonMapperFilenames = new();

    public List<string?> m_lodNames = new();

    public List<string?> m_mirroredSyncPointSubstringsA = new();

    public List<string?> m_mirroredSyncPointSubstringsB = new();

    public string? m_name;

    public string? m_rigName;

    public string? m_ragdollName;

    public string? m_behaviorFilename;

    public string? m_luaScriptOnCharacterActivated;

    public string? m_luaScriptOnCharacterDeactivated;

    public List<string?> m_luaFiles = new();


    public class FileNameMeshNamePair : IHavokObject
    {
        public string? m_fileName;

        public string? m_meshName;

    }


}

